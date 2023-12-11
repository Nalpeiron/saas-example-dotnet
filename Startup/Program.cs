using Auth0.AspNetCore.Authentication;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Orion.Api.Host.Startup.Extensions;
using ZentitleSaaSDemo.Zentitle;
using ZentitleSaaSDemo.Zentitle.Auth;
using ZentitleSaaSDemo.Settings;
using ZentitleSaaSDemo.Startup.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using ZentitleSaaSDemo.Models.Account;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

builder.Services.ConfigureHealthChecks();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ZentitleService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddHealthChecks();

if (builder.Configuration["Authentication"] == "Auth0")
{
    if (string.IsNullOrEmpty(builder.Configuration["Auth0:Domain"]) && string.IsNullOrEmpty(builder.Configuration["Auth0:ClientId"]))
    {
        throw new Exception("Auth0 - missing configuration");
    }

    builder.Services
        .AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = builder.Configuration["Auth0:Domain"];
            options.ClientId = builder.Configuration["Auth0:ClientId"];
            options.Scope = "openid profile email";
        });
}
else
{
    var users = builder.Configuration.GetSection("Users").Get<List<User>>();

    if (users == null || !users.Any())
    {
        throw new Exception("Users - missing configuration");
    }

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });
}

builder.Services.Configure<ZentitleOptions>(builder.Configuration.GetSection(ZentitleOptions.Zentitle));
builder.Services.Configure<EntitlementOptions>(builder.Configuration.GetSection(EntitlementOptions.Entitlement));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IAuthServiceClient, AuthService>(client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseForwardedHeaders();
    app.UseHsts();

    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next(context);
    });
}

app.UseRouting();

app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true
});

app.Run();