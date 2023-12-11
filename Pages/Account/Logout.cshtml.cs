using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ZentitleSaaSDemo.Zentitle;
using Microsoft.AspNetCore.Mvc;

namespace ZentitleSaaSDemo.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ZentitleService _service;
        public LogoutModel(ZentitleService service)
        {
            _service = service;
        }

        public async Task<IActionResult> OnGet()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _service.Deactivate();
            _service.RemoveCache();

            return Redirect("/");
        }
    }
}
