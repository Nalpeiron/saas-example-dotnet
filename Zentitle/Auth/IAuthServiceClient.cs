namespace ZentitleSaaSDemo.Zentitle.Auth;

public interface IAuthServiceClient
{
    Task<string> RequestAccessToken();
    Task InvalidateAccessToken();
}
