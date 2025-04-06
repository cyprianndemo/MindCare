#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MindCare.Models;
using Newtonsoft.Json;

namespace MindCare.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginRoleSelectionModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<ExternalLoginRoleSelectionModel> _logger;

        public ExternalLoginRoleSelectionModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginRoleSelectionModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; }

            // Student fields
            [Display(Name = "University")]
            public string University { get; set; }

            [Display(Name = "Course")]
            public string Course { get; set; }

            [Display(Name = "Year of Study")]
            public string YearOfStudy { get; set; }

            // Medical Professional fields
            [Display(Name = "National ID")]
            public string NationalId { get; set; }

            [Display(Name = "Licence Number")]
            public string LicenceNumber { get; set; }

            [Display(Name = "Hospital")]
            public string Hospital { get; set; }
        }

        public void OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            Email = TempData["ExternalLoginEmail"] as string;
            FirstName = TempData["ExternalLoginFirstName"] as string;
            LastName = TempData["ExternalLoginLastName"] as string;

            // Preserve TempData for the post action
            TempData.Keep("ExternalLoginEmail");
            TempData.Keep("ExternalLoginFirstName");
            TempData.Keep("ExternalLoginLastName");
            TempData.Keep("ExternalLoginInfo");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Get the login info from TempData
                var externalLoginInfoJson = TempData["ExternalLoginInfo"] as string;
                if (string.IsNullOrEmpty(externalLoginInfoJson))
                {
                    return RedirectToPage("./Login");
                }

                var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
                if (externalLoginInfo == null)
                {
                    return RedirectToPage("./Login");
                }

                var email = TempData["ExternalLoginEmail"] as string;
                var firstName = TempData["ExternalLoginFirstName"] as string;
                var lastName = TempData["ExternalLoginLastName"] as string;

                // Create a new user
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

                user.FirstName = firstName;
                user.LastName = lastName;
                user.Role = Input.Role;

                // Set role-specific fields
                if (Input.Role == "Student")
                {
                    user.University = Input.University;
                    user.Course = Input.Course;
                    user.YearOfStudy = Input.YearOfStudy;
                }
                else if (Input.Role == "Therapist" || Input.Role == "Psychiatrist")
                {
                    user.NationalId = Input.NationalId;
                    user.LicenceNumber = Input.LicenceNumber;
                    user.Hospital = Input.Hospital;
                }

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, externalLoginInfo);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", externalLoginInfo.LoginProvider);

                        // Assign the selected role
                        if (await _roleManager.RoleExistsAsync(Input.Role))
                        {
                            await _userManager.AddToRoleAsync(user, Input.Role);
                        }

                        // Sign in the user
                        await _signInManager.SignInAsync(user, isPersistent: false, externalLoginInfo.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            // Preserve TempData for redisplaying the form
            TempData.Keep("ExternalLoginEmail");
            TempData.Keep("ExternalLoginFirstName");
            TempData.Keep("ExternalLoginLastName");
            TempData.Keep("ExternalLoginInfo");

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}