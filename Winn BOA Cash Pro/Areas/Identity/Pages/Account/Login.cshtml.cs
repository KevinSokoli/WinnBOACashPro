// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Winn_BOA_Cash_Pro.Data;
using Winn_BOA_Cash_Pro.Models;

namespace Winn_BOA_Cash_Pro.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<LoginModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
            if (!User.Identity.IsAuthenticated)
            {
                //if user is not authenticated, redirect them to the Login
                var provider = "OpenIdConnect";
                var redirectUrl = Url.Page("./Login", pageHandler: "Callback", values: new { returnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return new ChallengeResult(provider, properties);
            }
            return Page();
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            //initialize variables
            var email = "";
            string empEmployeeId = null;
            bool NoErrors = true;
            IdentityResult identityResult = new();

            //set email variable to user's email
            if (info.ProviderDisplayName == "OpenIdConnect")
            {
                //OpenID spec doesn't recommend using preferred_username as a unique identifier because theoretically years from now someone else may take that username.
                //As of 2023, Winn does not delete accounts so it shouldn't be a concern.
                //If it becomes a concern, replace "preferred_username" with "email"
                email = info.Principal.FindFirstValue("preferred_username");
            }

            //set currentUser variable to the user in the Users table with the login email
            AppUser currentUser = _context.Users.Where(u => u.UserName.ToLower() == email.ToLower()).FirstOrDefault();

            //if the email exists in VwHrexports, set userEmployeeId variable
            if (_context.VwHrExports.Where(
                            u => u.Status != "TERMINATED" &&
                            u.BusinessEmail.ToLower() == email.ToLower()).Any())
            {
                empEmployeeId = _context.VwHrExports.Where(
                            u => u.Status != "TERMINATED" &&
                            u.BusinessEmail.ToLower() == email.ToLower())
                        .FirstOrDefault().EmployeeId;
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                switch ((FindUserEmail(currentUser) << 2) | (FindEmployeeEmail(email) << 1) | (FindEmployeeId(empEmployeeId)))
                {
                    case 7: //111 ("case 0b111:" also works)
                        //email exists in Users             email exists in HrExport                EmployeeId exists in Users
                        //no problems, correct email, correct employee ID - proceed

                        break;
                    case 6: //110
                        //email exists in Users             email exists in HrExport                EmployeeId does not exist in Users
                        //update existing user with Employee ID
                        currentUser.EmployeeId = empEmployeeId;
                        identityResult = await _userManager.UpdateAsync(currentUser);
                        if (!identityResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Unexpected error occurred setting the last login date" +
                                $" ({identityResult.ToString()}) for user with ID '{currentUser.Id}'.");
                        }

                        //replace incorrect EmployeeId GUIDs in Requests table (EmployeeId, ApproverId, CreatedById)
                        await _context.Database.ExecuteSqlRawAsync(
                            "exec spFixEmployeeID @OldEmployeeID = {0}, @NewEmployeeid = {1}",
                            currentUser.EmployeeId, empEmployeeId);

                        break;
                    case 4: //100
                        //email exists in Users             email does not exist in HrExport        EmployeeId does not exist in Users
                        //user is not an employee - display a cautionary message

                        break;
                    case 3: //011
                        //email does not exist in Users     email exists in HrExport                EmployeeId exists in Users
                        //update existing user with login email using Employee ID
                        currentUser = _context.Users.Where(u => u.EmployeeId == empEmployeeId).FirstOrDefault();
                        //currentUser.UserName = email;
                        //currentUser.NormalizedUserName = email.ToUpper();
                        //currentUser.Email = email;
                        //currentUser.NormalizedEmail = email.ToUpper();
                        var token = await _userManager.GenerateChangeEmailTokenAsync(currentUser, email);

                        await _userManager.ChangeEmailAsync(currentUser, email, token);

                        //await _userManager.UpdateAsync(currentUser);
                        break;
                    default: //000, 001, 010, 011, 101
                        //email does not exist in Users     email does not exist in HrExport        EmployeeId does not exist in Users
                        //create new user - use GUID instead of EmployeeID

                        //create a new user without an Employee Id
                        currentUser = new AppUser() { Created = DateTime.Now, CreatedBy = "System", Email = email, UserName = email };
                        identityResult = await _userManager.CreateAsync(currentUser);
                        if (!identityResult.Succeeded)
                        {
                            NoErrors = false;
                        }

                        //set the EmployeeId of the new user to the User ID (GUID)
                        currentUser.EmployeeId = currentUser.Id;
                        identityResult = await _userManager.UpdateAsync(currentUser);
                        if (!identityResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Unexpected error occurred setting the last login date" +
                                $" ({identityResult.ToString()}) for user with ID '{currentUser.Id}'.");
                        }
                        break;
                }

                //nothing is wrong - user is in database and has an EmployeeId, redirect to homepage
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            else
            {
                //sign in did not succeed
                switch ((FindUserEmail(currentUser) << 2) | (FindEmployeeEmail(email) << 1) | (FindEmployeeId(empEmployeeId)))
                {
                    case 2: //010
                        //email does not exist in Users     email exists in HrExport                EmployeeId does not exist in Users
                        //employee exists - create a new user
                        currentUser = new AppUser() { Created = DateTime.Now, CreatedBy = "System", Email = email, UserName = email, EmployeeId = empEmployeeId };
                        identityResult = await _userManager.CreateAsync(currentUser);
                        if (!identityResult.Succeeded)
                        {
                            NoErrors = false;
                        }
                        break;
                    default: //000, 001, 010, 011, 101
                        //email does not exist in Users     email does not exist in HrExport        EmployeeId does not exist in Users
                        //create new user - use GUID instead of EmployeeID

                        //create a new user without an Employee Id
                        currentUser = new AppUser() { Created = DateTime.Now, CreatedBy = "System", Email = email, UserName = email };
                        identityResult = await _userManager.CreateAsync(currentUser);
                        if (!identityResult.Succeeded)
                        {
                            NoErrors = false;
                        }

                        //set the EmployeeId of the new user to the User ID (GUID)
                        currentUser.EmployeeId = currentUser.Id;
                        identityResult = await _userManager.UpdateAsync(currentUser);
                        if (!identityResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Unexpected error occurred setting the last login date" +
                                $" ({identityResult.ToString()}) for user with ID '{currentUser.Id}'.");
                        }
                        break;
                }

                //create a login for the current user in AspNetUserLogins
                var externalLogin = _context.UserLogins.Where(u => u.LoginProvider == info.LoginProvider && u.UserId == currentUser.Id).FirstOrDefault();
                if (externalLogin == null)
                {
                    identityResult = await _userManager.AddLoginAsync(currentUser, info);

                    if (!identityResult.Succeeded) { NoErrors = false; }

                    foreach (var error in identityResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                //add user to the default User role
                await _userManager.AddToRoleAsync(currentUser, "User");
                //capture login information
                await _signInManager.SignInAsync(currentUser, isPersistent: false, info.LoginProvider);
            }

            return LocalRedirect(returnUrl);
        }

        private int FindUserEmail(AppUser currentUser)
        {
            var userEmailExists = 0;
            if (currentUser != null)
            {
                //user email exists in Users table
                userEmailExists = 1;
            }

            return userEmailExists;
        }

        private int FindEmployeeEmail(string email)
        {
            var empEmailExists = 0;
            if (_context.VwHrExports.Where(
                u => u.Status != "TERMINATED" &&
                u.BusinessEmail.ToLower() == email.ToLower()).Any())
            {
                //user email exists in Users table
                empEmailExists = 1;
            }

            return empEmailExists;
        }

        private int FindEmployeeId(string empEmployeeId)
        {
            var userEmpIdExists = 0;
            var employeeUser = _context.Users.Where(u => u.EmployeeId == empEmployeeId).Any();
            if (employeeUser)
            {
                //employee ID exists in Users table
                userEmpIdExists = 1;
            }

            return userEmpIdExists;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
