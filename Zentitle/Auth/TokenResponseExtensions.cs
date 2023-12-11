using IdentityModel.Client;
using System.IdentityModel.Tokens.Jwt;

namespace ZentitleSaaSDemo.Zentitle.Auth;

internal static class TokenResponseExtensions
{
    internal static TimeSpan? GetAccessTokenExpiresIn(this TokenResponse tokenResponse)
    {
        if (tokenResponse is null) throw new ArgumentNullException(nameof(tokenResponse));

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(tokenResponse.AccessToken);

        return token.ValidTo - DateTime.UtcNow;
    }

    internal static TimeSpan? GetRefreshTokenExpiresIn(this TokenResponse tokenResponse)
    {
        if (tokenResponse is null) throw new ArgumentNullException(nameof(tokenResponse));

        var refreshToken = tokenResponse.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken)) return null;

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(tokenResponse.RefreshToken);

        return token.ValidTo - DateTime.UtcNow;
    }

    internal static bool IsAccessTokenValid(this TokenResponse tokenResponse)
    {
        if (tokenResponse is null) throw new ArgumentNullException(nameof(tokenResponse));

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.ReadToken(tokenResponse.AccessToken);

        return accessToken.ValidTo > DateTime.UtcNow;
    }

    internal static bool IsRefreshTokenValid(this TokenResponse tokenResponse)
    {
        if (tokenResponse is null) throw new ArgumentNullException(nameof(tokenResponse));

        var refreshToken = tokenResponse.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken)) return false;

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(refreshToken);

        return token.ValidTo > DateTime.UtcNow;
    }
}
