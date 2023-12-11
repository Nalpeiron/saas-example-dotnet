using IdentityModel.Client;

namespace ZentitleSaaSDemo.Zentitle.Auth;

public static class IAuthCredentialAccessorExtensions
{
    internal static ClientCredentialsTokenRequest GetClientCredentialsTokenRequest(this AuthCredentials credentials)
    {
        ClientCredentialsTokenRequest tokenRequest = new()
        {
            Address = "protocol/openid-connect/token",
            ClientId = credentials.ClientId,
            ClientSecret = credentials.ClientSecret
        };

        return tokenRequest;
    }

    internal static Uri GetAuthority(this AuthCredentials credentials)
    {
        return new Uri(credentials.Authority);
    }
}
