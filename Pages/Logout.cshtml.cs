using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ZentitleSaaSDemo.Zentitle;

namespace ZentitleSaaSDemo.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ZentitleService _service;
        public LogoutModel(ZentitleService service)
        {
            _service = service;
        }

        public async Task OnGet()
        {
            await _service.Deactivate();
            _service.RemoveCache();
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri("/")
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
