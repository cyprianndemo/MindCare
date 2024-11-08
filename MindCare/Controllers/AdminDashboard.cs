using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MindCare.Models;
using MindCare.ViewModel;
using MindCare.ViewModels;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;

namespace MindCare.Controllers
{ 
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserActivityService _activityService;
        private readonly ApplicationDbContext _context;



        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, UserActivityService activityService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _activityService = activityService;
            _context = context;
        }
        public IActionResult Dashboard()
        {
            return View("Dashboard"); // Renders Views/Admin/Dashboard.cshtml
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = allRoles.Select(role => new RoleSelection
                {
                    RoleName = role.Name,
                    Selected = userRoles.Contains(role.Name)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.Roles.Where(r => r.Selected && !currentRoles.Contains(r.RoleName)).Select(r => r.RoleName);
            var rolesToRemove = currentRoles.Where(r => !model.Roles.Any(x => x.RoleName == r && x.Selected));

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            await _userManager.UpdateAsync(user);
            await _activityService.LogActivity(User.Identity.Name, "Edit User", $"Edited user: {model.UserName}");

            return RedirectToAction("ManageUsers");
        }

        // Delete user
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("ManageUsers");
        }
        public async Task<IActionResult> UserActivityReport()
        {
            // Retrieve user activities ordered by latest timestamp
            var activities = await _context.UserActivities
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(activities);
        }

        public IActionResult ViewReports()
        {
            // Logic for viewing reports
            return View();
        }

        public IActionResult SystemSettings()
        {
            // Logic for managing system settings
            return View();
        }
    }

}
