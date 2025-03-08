using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MindCare.Models;
using MindCare.ViewModel;
using MindCare.ViewModels;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using DinkToPdf.Contracts;
using OfficeOpenXml;

namespace MindCare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserActivityService _activityService;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ApplicationDbContext _context;
        private readonly IConverter _pdfConverter;
        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            UserActivityService activityService,
            ApplicationDbContext context,
            IConverter pdfConverter,
            ICompositeViewEngine viewEngine)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _activityService = activityService;
            _context = context;
            _viewEngine = viewEngine;
            _pdfConverter = pdfConverter;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<IActionResult> Dashboard()
        {         
            return View("Dashboard");
            
        }
        public async Task<IActionResult> ExportFinancialReport(string format)
        {
            // Get payment transaction summary
            var transactions = await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Calculate total payments received
            var totalPaymentsReceived = transactions.Sum(p => p.Amount);

            // Group payments by payment method
            var paymentMethodSummary = transactions
                .GroupBy(p => p.Method)
                .Select(g => new PaymentMethodSummary
                {
                    Method = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
                    Count = g.Count(),
                    Total = g.Sum(p => p.Amount)
                })
                .ToList();

            // Get monthly payment data for the past 6 months
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyPaymentData = transactions
                .Where(p => p.PaymentDate >= sixMonthsAgo)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new MonthlyPayment
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Package the data into a view model
            var viewModel = new FinancialTransactionsReportViewModel
            {
                TotalPaymentsReceived = totalPaymentsReceived,
                TotalTransactions = transactions.Count,
                SuccessfulTransactions = transactions.Count(p => p.Status.HasValue && p.Status.Value.ToString() == "Completed"),
                FailedTransactions = transactions.Count(p => p.Status.HasValue && p.Status.Value.ToString() == "Failed"),
                PaymentMethodSummary = paymentMethodSummary,
                MonthlyPaymentData = monthlyPaymentData,
                RecentTransactions = transactions.Take(10).ToList()
            };

            if (format.ToLower() == "pdf")
            {
                return await GeneratePdfReport(viewModel);
            }
            else if (format.ToLower() == "excel")
            {
                return GenerateExcelReport(viewModel);
            }

            // If an invalid format is specified, redirect back to the report page
            return RedirectToAction("FinancialTransactionsReport");
        }

        private async Task<IActionResult> GeneratePdfReport(FinancialTransactionsReportViewModel model)
        {
         
            using var memoryStream = new MemoryStream();

            var htmlContent = await RenderViewToStringAsync("FinancialTransactionsReportPDF", model);

            
            byte[] pdfBytes = System.Text.Encoding.UTF8.GetBytes("PDF content would be here");
            return File(pdfBytes, "application/pdf", "FinancialReport.pdf");
        }

        private IActionResult GenerateExcelReport(FinancialTransactionsReportViewModel model)
        {
            using var memoryStream = new MemoryStream();

            byte[] excelBytes = System.Text.Encoding.UTF8.GetBytes("Excel content would be here");
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FinancialReport.xlsx");
        }

        // Helper method to render a view to a string
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view");
            }

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
        // USER MANAGEMENT SECTION
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserManagementViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FirstName + " " + u.LastName,
                    
                })
                .ToListAsync();

            // Add roles to each user in the list
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                var roles = await _userManager.GetRolesAsync(appUser);
                user.Roles = string.Join(", ", roles);
            }

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
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
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
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.Roles.Where(r => r.Selected && !currentRoles.Contains(r.RoleName)).Select(r => r.RoleName);
            var rolesToRemove = currentRoles.Where(r => !model.Roles.Any(x => x.RoleName == r && x.Selected));

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            await _userManager.UpdateAsync(user);
            await _activityService.LogActivity(User.Identity.Name, "Edit User", $"Edited user: {model.UserName}");

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _activityService.LogActivity(User.Identity.Name, "Delete User", $"Deleted user: {user.UserName}");
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("ManageUsers");
        }

        // Add new user
        public IActionResult CreateUser()
        {
            var model = new CreateUserViewModel
            {
                Roles = _roleManager.Roles.Select(r => new RoleSelection
                {
                    RoleName = r.Name,
                    Selected = false
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                   
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.RoleName);
                    await _userManager.AddToRolesAsync(user, selectedRoles);
                    await _activityService.LogActivity(User.Identity.Name, "Create User", $"Created user: {model.Email}");

                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            model.Roles = _roleManager.Roles.Select(r => new RoleSelection
            {
                RoleName = r.Name,
                Selected = model.Roles.Any(mr => mr.RoleName == r.Name && mr.Selected)
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> UserActivityReport()
        {
            // Retrieve all user activities, including user info, ordered by the latest timestamp
            var activities = await _context.UserActivities
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(activities);
        }

        // REPORTS SECTION
        public IActionResult ViewReports()
        {
            return View();
        }

        public async Task<IActionResult> SystemPerformanceReport()
        {
            // Get users by roles
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var therapists = await _userManager.GetUsersInRoleAsync("Therapist");
            var psychiatrists = await _userManager.GetUsersInRoleAsync("Psychiatrist");

            // Count users by role
            var studentCount = students.Count;
            var therapistCount = therapists.Count;
            var psychiatristCount = psychiatrists.Count;
            var totalUsers = studentCount + therapistCount + psychiatristCount;

            // Get monthly active users (users who logged in within the last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var monthlyActiveUsers = await _context.UserActivities
                .Where(a => a.Timestamp >= thirtyDaysAgo && a.Action == "Login")
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            // Get session statistics
            var totalSessions = await _context.Appointments.CountAsync();
            var completedSessions = await _context.Appointments.CountAsync(a => a.Status == "Approved");
            var upcomingSessions = await _context.Appointments.CountAsync(a => a.Status == "Pending");
            var cancelledSessions = await _context.Appointments.CountAsync(a => a.Status == "Cancelled");

            // Get monthly session booking data for the past 6 months
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            // FIXED CODE: Move the string formatting to client side after retrieving the data
            var appointmentsData = await _context.Appointments
                .Where(a => a.Date >= sixMonthsAgo)
                .GroupBy(a => new { a.Date.Year, a.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    SessionCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Format the month string client-side after data is retrieved from the database
            var monthlySessionData = appointmentsData
                .Select(x => new MindCare.ViewModels.MonthlySession
                {
                    Month = $"{x.Year}-{x.Month:D2}",
                    SessionCount = x.SessionCount
                })
                .ToList();

            // System performance metrics (mocked data for demonstration)
            var systemPerformanceData = new SystemPerformanceMetrics
            {
                Uptime = 99.97,
                AverageResponseTime = 0.42,
                ErrorCount = 23,
                CriticalErrors = 2
            };

            // User feedback summary (mocked data for demonstration)
            var userFeedback = new List<FeedbackSummary>
    {
        new FeedbackSummary { Issue = "Scheduling conflicts", Count = 15 },
        new FeedbackSummary { Issue = "Video call quality", Count = 12 },
        new FeedbackSummary { Issue = "Payment processing", Count = 8 },
        new FeedbackSummary { Issue = "Login issues", Count = 5 }
    };

            // Package the data into a view model
            var viewModel = new SystemPerformanceReportViewModel
            {
                TotalUsers = totalUsers,
                StudentCount = studentCount,
                TherapistCount = therapistCount,
                PsychiatristCount = psychiatristCount,
                MonthlyActiveUsers = monthlyActiveUsers,
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                UpcomingSessions = upcomingSessions,
                CancelledSessions = cancelledSessions,
                MonthlySessionData = monthlySessionData,
                SystemPerformance = systemPerformanceData,
                UserFeedback = userFeedback
            };

            return View(viewModel);
        }

        public async Task<IActionResult> FinancialTransactionsReport()
        {
            // Get payment transaction summary
            var transactions = await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Calculate total payments received
            var totalPaymentsReceived = transactions.Sum(p => p.Amount);

            // Group payments by payment method
            var paymentMethodSummary = transactions
                .GroupBy(p => p.Method)
                .Select(g => new PaymentMethodSummary
                {
                    Method = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
                    Count = g.Count(),
                    Total = g.Sum(p => p.Amount)
                })
                .ToList();

            // Get monthly payment data for the past 6 months
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyPaymentData = transactions
                .Where(p => p.PaymentDate >= sixMonthsAgo)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new MonthlyPayment
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Get payment status summary
            var paymentStatusSummary = transactions
                .GroupBy(p => p.Status)
                .Select(g => new PaymentStatusSummary
                {
                    Status = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown", // Convert nullable enum to string
                    Count = g.Count(),
                    Total = g.Sum(p => p.Amount)
                })
                .ToList();


            // Package the data into a view model
            var viewModel = new FinancialTransactionsReportViewModel
            {
                TotalPaymentsReceived = totalPaymentsReceived,
                TotalTransactions = transactions.Count,
                SuccessfulTransactions = transactions.Count(p => p.Status.HasValue && p.Status.Value.ToString() == "Completed"),
                FailedTransactions = transactions.Count(p => p.Status.HasValue && p.Status.Value.ToString() == "Failed"),
                PaymentMethodSummary = paymentMethodSummary,
                MonthlyPaymentData = monthlyPaymentData,
                PaymentStatusSummary = paymentStatusSummary,
                RecentTransactions = transactions.Take(10).ToList()
            };

            return View(viewModel);
        }

        // In AdminController.cs
        public async Task<IActionResult> SystemSettings()
        {
            // Retrieve current system settings from database
            var settings = await _context.SystemSettings.FirstOrDefaultAsync()
                ?? new SystemSettings(); // Create default if none exists

            var viewModel = new SystemSettingsViewModel
            {
                ApplicationName = settings.ApplicationName,
                SupportEmail = settings.SupportEmail,
                MaintenanceMode = settings.MaintenanceMode,
                SessionDurationMinutes = settings.SessionDurationMinutes,
                MaxAppointmentsPerDay = settings.MaxAppointmentsPerDay,
                AllowSelfRegistration = settings.AllowSelfRegistration,
                EnabledPaymentMethods = settings.EnabledPaymentMethods,
                VideoCallProvider = settings.VideoCallProvider,
                DefaultCurrencySymbol = settings.DefaultCurrencySymbol,
                SystemTimeZone = settings.SystemTimeZone,
                NotificationSettings = settings.NotificationSettings
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SystemSettings(SystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get existing settings or create new
                var settings = await _context.SystemSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new SystemSettings();
                    _context.SystemSettings.Add(settings);
                }

                // Update settings properties
                settings.ApplicationName = model.ApplicationName;
                settings.SupportEmail = model.SupportEmail;
                settings.MaintenanceMode = model.MaintenanceMode;
                settings.SessionDurationMinutes = model.SessionDurationMinutes;
                settings.MaxAppointmentsPerDay = model.MaxAppointmentsPerDay;
                settings.AllowSelfRegistration = model.AllowSelfRegistration;
                settings.EnabledPaymentMethods = model.EnabledPaymentMethods;
                settings.VideoCallProvider = model.VideoCallProvider;
                settings.DefaultCurrencySymbol = model.DefaultCurrencySymbol;
                settings.SystemTimeZone = model.SystemTimeZone;
                settings.NotificationSettings = model.NotificationSettings;

                await _context.SaveChangesAsync();
                await _activityService.LogActivity(User.Identity.Name, "Update Settings", "Updated system settings");

                TempData["SuccessMessage"] = "System settings updated successfully!";
                return RedirectToAction("SystemSettings");
            }

            return View(model);
        }
    }
}