using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ZentitleSaaSDemo.Settings;

namespace ZentitleSaaSDemo.Zentitle.Auth;

public class AuthService : IAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private readonly IMemoryCache _cache;
    private readonly ClientCredentialsTokenRequest _tokenRequest;
    private const string TokenKey = "ClientRealmToken";
    private AuthCredentials? _credentials = null;
    private readonly ZentitleOptions _options;

    public AuthService(
        HttpClient httpClient,
        IOptions<ZentitleOptions> options,
        ILogger<AuthService> logger,
        IMemoryCache memoryCache

        )
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _credentials = GetCredentials();
        
        _tokenRequest = _credentials.GetClientCredentialsTokenRequest();
        _httpClient.BaseAddress = _credentials.GetAuthority();
    }

    public async Task<string> RequestAccessToken()
    {
        var tokenExists = _cache.TryGetValue(TokenKey, out TokenResponse token);

        if (!tokenExists)
        {
            token = await RequestNewAccessToken();

            UpdateTokenInCache(tokenExists, token);
        }        
        else if (!token.IsAccessTokenValid())
        {
            if (!token.IsRefreshTokenValid())
            {
                token = await RequestNewAccessToken();
            }
            else
            {
                token = await RefreshToken(token);
            }

            UpdateTokenInCache(tokenExists, token);
        }

        return token.AccessToken;
    }

    public Task InvalidateAccessToken()
    {
        _cache.Remove(TokenKey);

        return Task.CompletedTask;
    }

    private async Task<TokenResponse> RequestNewAccessToken()
    {
        var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(_tokenRequest);

        if (tokenResponse.IsError)
        {
            _logger.LogError(tokenResponse.Error);
            throw new HttpRequestException("Something went wrong while requesting the access token");
        }

        return tokenResponse;
    }

    private async Task<TokenResponse> RefreshToken(TokenResponse token)
    {
        var refreshTokenRequest = CreateRefreshTokenRequest(token);
        var tokenResponse = await _httpClient.RequestRefreshTokenAsync(refreshTokenRequest);

        if (tokenResponse.IsError)
        {
            _logger.LogError(tokenResponse.Error);
        }
        else
        {
            tokenResponse = await RequestNewAccessToken();
        }

        return tokenResponse;
    }

    private void UpdateTokenInCache(bool tokenExists, TokenResponse token)
    {
        if (tokenExists)
        {
            _cache.Remove(TokenKey);
        }

        var entry = _cache.CreateEntry(TokenKey);
        var refreshTokenExpiresIn = token.GetRefreshTokenExpiresIn();

        if (refreshTokenExpiresIn.HasValue)
        {
            entry.SetSlidingExpiration(TimeSpan.FromSeconds(60));
            entry.AbsoluteExpirationRelativeToNow = refreshTokenExpiresIn;
        }
        else
        {
            var accessTokenExpiresIn = token.GetAccessTokenExpiresIn();

            entry.AbsoluteExpirationRelativeToNow = accessTokenExpiresIn;
        }

        entry.Value = token;
    }
    
    private RefreshTokenRequest CreateRefreshTokenRequest(TokenResponse token)
    {
        token = token ?? throw new ArgumentNullException(nameof(token));

        RefreshTokenRequest refreshTokenRequest = new()
        {
            Address = _tokenRequest.Address,
            ClientId = _tokenRequest.ClientId,
            ClientSecret = _tokenRequest.ClientSecret,
            RefreshToken = token.RefreshToken
        };

        return refreshTokenRequest;
    }
    
    private AuthCredentials GetCredentials()
    {
        return _credentials ??= new AuthCredentials(
            _options.AuthServiceUrl,
            _options.ClientId,
            _options.ClientSecret
        );
    }
}
