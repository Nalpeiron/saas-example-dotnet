namespace ZentitleSaaSDemo.Utils
{
    public static class ConfigurationExtensions
    {
        public static bool Auth0Authentication(this IConfiguration configuration)
        {
            return configuration["Authentication"]?.ToLower() == "auth0";
        }
    }
}
