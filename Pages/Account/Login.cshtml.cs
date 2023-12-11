using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ZentitleSaaSDemo.Models.Account;
using ZentitleSaaSDemo.Utils;

namespace ZentitleSaaSDemo.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Models.Account.LoginModel Model { get; set; }
        public string ErrorMessage = string.Empty;
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Model = new Models.Account.LoginModel();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var users = _configuration.GetSection("Users").Get<List<User>>();
            var loggedUser = users.Where(x => x.Email == Model.Email && x.Password == Model.Password).SingleOrDefault();

            if (loggedUser != null)
            {
                return await LoginUser(loggedUser);
            }
            else
            {
                ModelState.AddModelError("Password", "Wrong user or password");
                return Page();
            }
        }

        private async Task<IActionResult> LoginUser(User loggedUser)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loggedUser.Email!),
                new Claim(ConstValues.ActivationCodeClaim, loggedUser.ActivationCode!),
                new Claim(ClaimTypes.Email, loggedUser.Email!)
            };

            var principal = new ClaimsPrincipal();

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            principal.AddIdentity(claimsIdentity);

            await HttpContext.SignInAsync(principal);

            return Redirect("/");
        }
    }
}
