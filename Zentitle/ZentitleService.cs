using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ZentitleSaaSDemo.Settings;
using ZentitleSaaSDemo.Zentitle.Auth;

namespace ZentitleSaaSDemo.Zentitle;

public sealed class ZentitleService
{
    private readonly IAuthServiceClient _authService;
    private readonly ZentitleOptions _zentitleOptions;
    private readonly EntitlementOptions _entitlementOptions;
    private readonly IMemoryCache _cache;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly AuthUserDataService _authUserDataService;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string CompanyAttributeKey = "CompanyName";
    private const string PlanNameAttributeKey = "PlanName";
    private const int MaxSeatIdLength = 50;
    private string? _tokenKey;
    public ActivationStateModel? StateModel { get; private set; }
    public string? LicenseJson { get; private set; }
    public bool HasLicense => StateModel != null;
    public static string ElementPoolKey => "EP1";
    public static string FloatingFeatureKey => "FF1";
    public static string ConsumptionTokenKey => "CT1";
    public ZentitleServiceException? Exception { get; private set; }

    public string CompanyName => GetAttributeValue(CompanyAttributeKey);
    public string PlanName => GetAttributeValue(PlanNameAttributeKey);

    public ZentitleService(
        IHttpClientFactory httpClientFactory,
        IOptions<ZentitleOptions> zentitleOptions,
        IOptions<EntitlementOptions> entitlementOptions,
        IMemoryCache cache,
        AuthUserDataService authUserDataService,
        AuthenticationStateProvider authenticationStateProvider,
        IAuthServiceClient authServiceClient)
    {
        _authUserDataService = authUserDataService ?? throw new ArgumentNullException(nameof(authUserDataService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _entitlementOptions = entitlementOptions.Value ?? throw new ArgumentNullException(nameof(entitlementOptions));
        _zentitleOptions = zentitleOptions.Value ?? throw new ArgumentNullException(nameof(zentitleOptions));
        _authService = authServiceClient ?? throw new ArgumentNullException(nameof(authServiceClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _authenticationStateProvider = authenticationStateProvider ??
                                       throw new ArgumentNullException(nameof(authenticationStateProvider));
    }

    public void ClearException()
    {
        Exception = null;
        NotifyStateChanged();
    }

    public async Task GetLicense()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            return;
        }

        StateModel = await GetActivationState();
        if (StateModel != null)
        {
            LicenseJson = JsonConvert.SerializeObject(StateModel, Formatting.Indented);
        }

        NotifyStateChanged();
    }

    public async Task Deactivate()
    {
        var tokenExists = _cache.TryGetValue(await TokenKey(), out ActivationModel cachedActivationModel);

        if (!(tokenExists && cachedActivationModel!.IsAccessTokenValid()))
        {
            return;
        }

        var activationsClient = await GetActivationsClient();
        try
        {
            await activationsClient.DeleteAsync(cachedActivationModel.Id, true);
        }
        catch (ApiException exception)
        {
            Exception = new ZentitleServiceException(exception);
        }
        catch (Exception exception)
        {
            Exception = new ZentitleServiceException(exception);
        }
    }

    public IEnumerable<string> ActiveFeatures()
    {
        if (StateModel == null) return Array.Empty<string>();
        return StateModel.Features.Where(x => x.Active == 1).Select(x => x.Key).ToList();
    }


    public bool HasFeature(string featureKey)
    {
        return ActiveFeatures().Any(x => x == featureKey);
    }

    public bool FeatureExists(string featureKey)
    {
        return Features().Any(x => x == featureKey);
    }

    public ActivationFeatureModel GetElementPoolFeature()
    {
        return FeatureStateModel(ElementPoolKey);
    }

    public ActivationFeatureModel GetConsumptionToken()
    {
        return FeatureStateModel(ConsumptionTokenKey);
    }

    public ActivationFeatureModel GetFloatingFeature()
    {
        return FeatureStateModel(FloatingFeatureKey);
    }

    public async Task<ActivationFeatureModel> ReturnFloatingFeature()
    {
        await ReturnFeature(1, FloatingFeatureKey);
        return GetFloatingFeature();
    }

    public async Task<ActivationFeatureModel> CheckoutFloatingFeature()
    {
        await CheckoutFeature(1, FloatingFeatureKey);
        return GetFloatingFeature();
    }

    public async Task<ActivationFeatureModel> CheckoutConsumptionToken()
    {
        await CheckoutFeature(1, ConsumptionTokenKey);
        return GetConsumptionToken();
    }

    public async Task<ActivationFeatureModel> ReturnElementPoolFeature(int amount)
    {
        await ReturnFeature(amount, ElementPoolKey);
        return GetElementPoolFeature();
    }

    public async Task<ActivationFeatureModel> CheckoutElementPoolFeature(int amount)
    {
        await CheckoutFeature(amount, ElementPoolKey);
        return GetElementPoolFeature();
    }

    public async Task RemoveCache()
    {
        var tokenKey = await TokenKey();
        var tokenExists = _cache.TryGetValue(tokenKey, out _);
        if (tokenExists)
        {
            _cache.Remove(tokenKey);
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();

    private string GetAttributeValue(string key)
    {
        return HasLicense
            ? StateModel?.Attributes.FirstOrDefault(x => x.Key == key)?.Value ?? string.Empty
            : string.Empty;
    }

    private IEnumerable<string> Features()
    {
        if (StateModel == null) return Array.Empty<string>();
        return StateModel.Features.Select(x => x.Key).ToList();
    }

    private ActivationFeatureModel FeatureStateModel(string key)
    {
        if (StateModel == null) return new ActivationFeatureModel();
        return StateModel.Features.SingleOrDefault(x => x.Key == key) ?? new ActivationFeatureModel();
    }

    private async Task ReturnFeature(int amount, string key)
    {
        var activationsClient = await GetActivationsClient();
        var seatId = await RequestSeat();
        var request = new ReturnEntitlementFeatureApiRequest { Amount = amount, Key = key };
        try
        {
            await activationsClient.ReturnFeatureAsync(seatId, request);
        }
        catch (ApiException exception)
        {
            Exception = new ZentitleServiceException(exception);
        }
        catch (Exception exception)
        {
            Exception = new ZentitleServiceException(exception);
        }

        await GetLicense();
    }

    private async Task CheckoutFeature(int amount, string key)
    {
        var activationsClient = await GetActivationsClient();
        var seatId = await RequestSeat();
        var request = new CheckoutEntitlementFeatureApiRequest { Amount = amount, Key = key };
        try
        {
            await activationsClient.CheckoutFeatureAsync(seatId, request);
        }
        catch (ApiException exception)
        {
            Exception = new ZentitleServiceException(exception);
        }
        catch (Exception exception)
        {
            Exception = new ZentitleServiceException(exception);
        }

        await GetLicense();
    }

    private async Task<ActivationStateModel?> GetActivationState()
    {
        ActivationStateModel? result = null;
        try
        {
            var seatId = await RequestSeat();
            var httpClient = await GetHttpClient();
            var activationsClient = new ActivationsClient(_zentitleOptions.ZentitleUrl, httpClient);
            result = await activationsClient.GetActivationStateAsync(seatId);
        }
        catch (ApiException exception)
        {
            Exception = new ZentitleServiceException(exception);
        }
        catch (Exception exception)
        {
            Exception = new ZentitleServiceException(exception);
        }

        return result;
    }

    private async Task<ActivationModel?> SeatActivate()
    {
        var authUserData = await _authUserDataService.GetAuthUserData();
        var email = authUserData.Email;
        var activationCode = authUserData.ActivationCode;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(activationCode))
        {
            return null;
        }
        var seatId = email.Length <= MaxSeatIdLength ? email : email[..MaxSeatIdLength];
        var activationsClient = await GetActivationsClient();
        var r = new ActivateEntitlementApiRequest()
        {
            ActivationCode = activationCode,
            ProductId = _entitlementOptions.ProductId,
            SeatId = seatId
        };

        var result = await activationsClient.ActivateAsync(r);
        await UpdateTokenInCache(false, result);

        return result;
    }

    private async Task<ActivationsClient> GetActivationsClient()
    {
        var httpClient = await GetHttpClient();
        var activationsClient = new ActivationsClient(_zentitleOptions.ZentitleUrl, httpClient);
        return activationsClient;
    }

    private async Task<HttpClient> GetHttpClient()
    {
        var token = await _authService.RequestAccessToken();
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        httpClient.DefaultRequestHeaders.Add("N-TenantId", _zentitleOptions.TenantId);
        return httpClient;
    }

    private async Task<string?> RequestSeat()
    {
        var tokenKey = TokenKey();
        var tokenExists = _cache.TryGetValue(tokenKey, out ActivationModel cachedActivationModel);

        if (tokenExists && cachedActivationModel!.IsAccessTokenValid())
        {
            return cachedActivationModel.Id ?? string.Empty;
        }

        var activationModel = await SeatActivate();
        if (activationModel is null)
        {
            return string.Empty;
        }

        await UpdateTokenInCache(tokenExists, activationModel);

        return activationModel.Id;
    }

    private async Task UpdateTokenInCache(bool tokenExists, ActivationModel token)
    {
        var tokenKey = await TokenKey();
        if (tokenExists)
        {
            _cache.Remove(tokenKey);
        }

        _cache.Set(tokenKey, token, token.GetSeatExpiresIn(1));
    }

    private async Task<string> TokenKey()
    {
        if(_tokenKey == null)
        {
            var userData = await _authUserDataService.GetAuthUserData();
            _tokenKey = $"ZentitleSeatToken-{userData.Email}-{userData.ActivationCode}";
        }

        return _tokenKey;
    }
}