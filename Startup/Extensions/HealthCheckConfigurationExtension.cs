namespace Orion.Api.Host.Startup.Extensions;

public static class HealthCheckConfigurationExtension
{
    public static void ConfigureHealthChecks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHealthChecks();
    } 
}