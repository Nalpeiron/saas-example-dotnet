namespace ZentitleSaaSDemo.Settings;

public class ZentitleOptions
{
    public const string Zentitle = "Zentitle";

    public string AuthServiceUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ZentitleUrl { get; set; } = string.Empty;
}