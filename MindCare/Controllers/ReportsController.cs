/*using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.Services;

namespace MindCare.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
            IServiceProvider serviceProvider,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Get user's role
            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault() ?? "Student";

            // Get available reports for this user
            var reports = await _context.Reports
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();

            ViewBag.UserRole = userRole;

            return View(reports);
        }

        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null || report.UserId != user.Id)
            {
                return NotFound();
            }

            // Mark as viewed
            report.IsViewed = true;
            await _context.SaveChangesAsync();

            // Return the report file
            return PhysicalFile(report.ReportPath, GetContentType(report.ReportPath), Path.GetFileName(report.ReportPath));
        }

        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Get user's role
            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault() ?? "Student";

            // Current month and year
            var month = DateTime.Now.Month.ToString();
            var year = DateTime.Now.Year.ToString();

            // Generate report based on user role
            string reportPath = "";

            using (var scope = _serviceProvider.CreateScope())
            {
                switch (userRole)
                {
                    case "Student":
                        var studentReportGenerator = scope.ServiceProvider.GetRequiredService<StudentReportGenerator>();
                        reportPath = await studentReportGenerator.GenerateReportAsync(user.Id, month, year);
                        break;
                    case "Therapist":
                        var therapistReportGenerator = scope.ServiceProvider.GetRequiredService<TherapistReportGenerator>();
                        reportPath = await therapistReportGenerator.GenerateReportAsync(user.Id, month, year);
                        break;
                    *//*case "Psychiatrist":
                        var psychiatristReportGenerator = scope.ServiceProvider.GetRequiredService<PsychiatristReportGenerator>();
                        reportPath = await psychiatristReportGenerator.GenerateReportAsync(user.Id, month, year);
                        break;
                    case "Admin":
                        var adminReportGenerator = scope.ServiceProvider.GetRequiredService<AdminReportGenerator>();
                        reportPath = await adminReportGenerator.GenerateReportAsync(user.Id, month, year);*/
                        /*break;*//*
                }
            }

            return RedirectToAction("Index");
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".pdf":
                    return "application/pdf";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                default:
                    return "application/octet-stream";
            }
        }
    }

}
*/