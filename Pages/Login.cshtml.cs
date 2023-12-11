using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Auth0.AspNetCore.Authentication;
using ZentitleSaaSDemo.Zentitle;

namespace ZentitleSaaSDemo.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ZentitleService _service;

        public LoginModel(ZentitleService service)
        {
            _service = service;
        }

        public async Task OnGet(string redirectUri)
        {
            _service.RemoveCache();
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }
    }
}
