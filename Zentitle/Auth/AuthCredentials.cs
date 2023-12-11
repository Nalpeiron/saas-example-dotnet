namespace ZentitleSaaSDemo.Zentitle.Auth;

public class AuthCredentials
{
    public AuthCredentials(string authority, string clientId, string clientSecret)
    {
        Authority = authority.EndsWith('/') ? authority : $"{authority}/";
        ClientId = clientId;
        ClientSecret = clientSecret;
    }

    public string Authority { get; private set; }
    public string ClientId { get; private set; }
    public string ClientSecret { get; private set; }
}
