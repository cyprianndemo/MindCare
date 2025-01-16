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
            // Retrieve all user activities, including user info, ordered by the latest timestamp
            var activities = await _context.UserActivities
                .Include(a => a.User)
                .Where(a => a.Action == "Login" ||
                            a.Action == "Logout" ||
                            a.Action == "BookSession" ||
                            a.Action == "EditSession" ||
                            a.Action == "CancelAppointment")
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
        public async Task<IActionResult> SystemUsageReport()
        {
            // Get monthly session data
            var monthlySessionData = await _context.Appointments
                .GroupBy(a => new { a.Date.Year, a.Date.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    SessionCount = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Calculate totals for display cards
            var totalSessions = monthlySessionData.Sum(x => x.SessionCount);
            var currentMonthSessions = monthlySessionData
                .Where(x => x.Year == DateTime.Now.Year && x.Month == DateTime.Now.Month)
                .Sum(x => x.SessionCount);

            // Populate ViewModel
            var viewModel = new SystemUsageReportViewModel
            {
                TotalSessions = totalSessions,
                CurrentMonthSessions = currentMonthSessions,
                MonthlySessionData = monthlySessionData.Select(x => new MonthlySession
                {
                    Month = $"{x.Year}-{x.Month:D2}",
                    SessionCount = x.SessionCount
                }).ToList()
            };

            return View(viewModel);
        }


        public async Task<IActionResult> PerformanceReport()
        {
            // Get users by roles
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var therapists = await _userManager.GetUsersInRoleAsync("Therapist");
            var psychiatrists = await _userManager.GetUsersInRoleAsync("Psychiatrist");

            // Count users by role
            var studentCount = students.Count;
            var therapistCount = therapists.Count;
            var psychiatristCount = psychiatrists.Count;

            // Count appointment sessions and their statuses
            var totalSessions = await _context.Appointments.CountAsync();
            var completedSessions = await _context.Appointments.CountAsync(a => a.Status == "Completed");
            var upcomingSessions = await _context.Appointments.CountAsync(a => a.Status == "Approved");

            // Package the data into a view model
            var viewModel = new PerformanceReportViewModel
            {
                StudentCount = studentCount,
                TherapistCount = therapistCount,
                PsychiatristCount = psychiatristCount,
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                UpcomingSessions = upcomingSessions
            };

            return View(viewModel);
        }


    }

}
