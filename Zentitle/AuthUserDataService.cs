using Microsoft.AspNetCore.Components.Authorization;
using ZentitleSaaSDemo.Utils;

namespace ZentitleSaaSDemo.Zentitle
{
    public class AuthUserDataService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthUserDataService(AuthenticationStateProvider authenticationStateProvider) {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<AuthUserData> GetAuthUserData()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var activationCode = user.FindFirst(ConstValues.ActivationCodeClaim)?.Value;
            var picture = user.FindFirst(c => c.Type.Equals("picture"))?.Value;
            var username = authState.User.Identity?.Name ?? string.Empty;

            return new AuthUserData()
            { Email = email, Picture = picture, Username = username, ActivationCode = activationCode };
        }
    }
}
