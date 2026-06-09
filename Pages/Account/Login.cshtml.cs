using MediSphere.Models;
using MediSphere.Resources;
using MediSphere.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Localization;

namespace MediSphere.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public LoginModel(
            SignInManager<UserModel> signInManager,
            ILogger<LoginModel> logger,
            IStringLocalizer<SharedResources> localizer)
        {
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
        }

        [BindProperty, Required]
        public LoginViewModel CredentialModel { get; set; }

        public void OnGet()
        {
        
        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(CredentialModel.Email, CredentialModel.Password, CredentialModel.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                    _logger.LogInformation("User successfully logged in");

                    return RedirectToPage("/Home/Dashboard"); // Redirect to the main dashboard.
                    }
                
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { RememberMe = CredentialModel.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }

                ModelState.AddModelError(string.Empty, _localizer["InvalidCredentials"]);
            }

            return Page();
        }

    }
}
