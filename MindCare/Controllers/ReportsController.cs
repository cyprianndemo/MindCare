using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.Services;
using MindCare.TherapistReport;
using System.Security.Claims;

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

        // Add this method to TherapistController.cs
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> Report()
        {
            try
            {
                // Get the logged-in Therapist's ID
                var therapistId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get therapist details
                var therapist = await _context.Users.FirstOrDefaultAsync(u => u.Id == therapistId);
                if (therapist == null)
                {
                    TempData["Error"] = "Therapist information not found.";
                    return RedirectToAction("Index");
                }

                var therapistName = $"{therapist.FirstName} {therapist.LastName}";

                // Get all appointments for this therapist
                var currentDate = DateTime.UtcNow;

                // Get total numbers
                var totalSessions = await _context.Appointments
                    .Where(a => a.TherapistId == therapistId)
                    .CountAsync();

                var pendingSessions = await _context.Appointments
                    .Where(a => a.TherapistId == therapistId && a.Status == "Pending")
                    .CountAsync();

                var approvedSessions = await _context.Appointments
                    .Where(a => a.TherapistId == therapistId && a.Status == "Approved")
                    .CountAsync();

                // Get upcoming pending appointments (ordered by date)
                var upcomingPendingAppointments = await _context.Appointments
                    .Where(a => a.TherapistId == therapistId &&
                               a.Status == "Pending" &&
                               a.StartTime > currentDate)
                    .Join(
                        _context.Users,
                        appointment => appointment.StudentId,
                        user => user.Id,
                        (appointment, user) => new TherapistAppointmentInfo
                        {
                            AppointmentId = appointment.AppointmentId,
                            StartTime = appointment.StartTime,
                            EndTime = appointment.EndTime,
                            StudentName = $"{user.FirstName} {user.LastName}",
                            StudentEmail = user.Email,
                            Status = appointment.Status
                        }
                    )
                    .OrderBy(a => a.StartTime)
                    .Take(10) // Limit to 10 upcoming appointments
                    .ToListAsync();

                // Get upcoming approved appointments (ordered by date)
                var upcomingApprovedAppointments = await _context.Appointments
                    .Where(a => a.TherapistId == therapistId &&
                               a.Status == "Approved" &&
                               a.StartTime > currentDate)
                    .Join(
                        _context.Users,
                        appointment => appointment.StudentId,
                        user => user.Id,
                        (appointment, user) => new TherapistAppointmentInfo
                        {
                            AppointmentId = appointment.AppointmentId,
                            StartTime = appointment.StartTime,
                            EndTime = appointment.EndTime,
                            StudentName = $"{user.FirstName} {user.LastName}",
                            StudentEmail = user.Email,
                            Status = appointment.Status
                        }
                    )
                    .OrderBy(a => a.StartTime)
                    .Take(10) // Limit to 10 upcoming appointments
                    .ToListAsync();

                var reportViewModel = new TherapistReportViewModel
                {
                    TherapistName = therapistName,
                    TotalSessions = totalSessions,
                    PendingSessions = pendingSessions,
                    ApprovedSessions = approvedSessions,
                    UpcomingPendingAppointments = upcomingPendingAppointments,
                    UpcomingApprovedAppointments = upcomingApprovedAppointments
                };

                return View(reportViewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Report: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = "An error occurred while generating the report.";
                return RedirectToAction("Index");
            }
        }

    }
}
