using Serilog;
using Serilog.Formatting.Compact;

namespace ZentitleSaaSDemo.Startup.Extensions;

public static class LoggingConfigurationBuilderExtensions
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDevelopment)
        {
            builder.Host.UseSerilog((_, lc) => lc
                .Enrich.FromLogContext()
                .WriteTo.Console());
        }
        else
        {
            builder.Host.UseSerilog((_, lc) => lc
                .Enrich.FromLogContext()
                .WriteTo.Console(new CompactJsonFormatter()));
        }
        
    }
}