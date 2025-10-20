using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PhotoGallery.Pages
{
    public class LoginModel(SignInManager<IdentityUser> signInManager,
        ILogger<LoginModel> logger,
        UserManager<IdentityUser> userManager,
        IHttpContextAccessor accessor) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        public string ReturnUrl { get; set; } = null!;

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "UserName is required")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = string.IsNullOrEmpty(returnUrl) ? Url.Content("~/") : returnUrl;

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Photos/List");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await signInManager.PasswordSignInAsync(Input.UserName, Input.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(Input.UserName);
                if (user != null)
                {
                    var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";

                    accessor?.HttpContext?.Response.Cookies.Append("RoleName", role);
                    accessor?.HttpContext?.Response.Cookies.Append("UserId", user.Id);

                    logger.LogInformation("User logged in. UserId: {UserId}, UserName: {UserName}, Role: {Role}",
                        user.Id, user.UserName, role);

                    // Redirect after successful login
                    return RedirectToPage("/Photos/List");
                }
            }

            ErrorMessage = "Invalid username or password.";
            logger.LogWarning("Failed login attempt for {Username}.", Input.UserName);
            return Page();
        }
    }
}
