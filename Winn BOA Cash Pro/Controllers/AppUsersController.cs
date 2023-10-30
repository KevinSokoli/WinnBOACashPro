using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winn_BOA_Cash_Pro.Data;
using Winn_BOA_Cash_Pro.Models;
using Winn_BOA_Cash_Pro.Models.ViewModels;

namespace Winn_BOA_Cash_Pro.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AppUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppUsersController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (AppUser user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Created = user.Created,
                    CreatedBy = user.CreatedBy,
                    Roles = await GetUserRoles(user)
                };
                userRolesViewModel.Add(thisViewModel);
            }
            PopulateDropdownData();
            return View(userRolesViewModel);
        }

        private void PopulateDropdownData()
        {
            #region Populate Employee Names
            var employeeQuery = _context.VwHrExports
                .Where(s => s.Status != "TERMINATED")
                .OrderBy(s => s.LastName)
                .Select(w => new
                {
                    EmployeeId = w.EmployeeId,
                    EmployeeName = string.Format("{0}|{1}|{2}", w.FullName, w.JobTitle, w.BusinessEmail)
                })
                .ToList();
            ViewBag.EmployeeList = new SelectList(employeeQuery, "EmployeeId", "EmployeeName");
            #endregion
        }

        private async Task<List<string>> GetUserRoles(AppUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles.ToList())
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddAsAdmin(string adminEmail, string adminEmployeeId)
        {
            //create appUser variable
            AppUser appUser = new();
            if (ModelState.IsValid)
            {
                //if user does not exist
                if (await _userManager.FindByEmailAsync(adminEmail) == null)
                {
                    //create a new user
                    appUser = new AppUser { UserName = adminEmail, Email = adminEmail, CreatedBy = User.Identity.Name, Created = DateTime.Now, EmailConfirmed = true, EmployeeId = adminEmployeeId };
                    await _userManager.CreateAsync(appUser);
                }
                else
                {
                    //set the appUser variable to the user with the existing adminEmail
                    appUser = await _userManager.FindByEmailAsync(adminEmail);
                }

                //add the user to the Admin role
                await _userManager.AddToRoleAsync(appUser, "Admin");

                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
