using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.ViewModel;
using System.Security.Claims;

namespace MindCare.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentReportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _userManager.FindByIdAsync(userId);

            if (student == null)
            {
                return NotFound();
            }

            // Get therapy appointments
            var therapyAppointments = await _context.Appointments
                .Include(a => a.Therapist)
                .Where(a => a.StudentId == userId &&
                        a.TherapistId != null &&
                        a.PsychiatristId == null)
                .ToListAsync();

            // Get psychiatry appointments
            var psychiatryAppointments = await _context.Appointments
                .Include(a => a.Psychiatrist)
                .Where(a => a.StudentId == userId &&
                        a.PsychiatristId != null)
                .ToListAsync();

            // Get medications prescribed to the student
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Medication)
                .Include(p => p.Psychiatrist)
                .Where(p => p.StudentId == userId)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            // Construct the view model
            var reportViewModel = new StudentReportViewModel
            {
                Student = student,
                StudentName = $"{student.FirstName} {student.LastName}",

                // Therapy statistics
                TotalTherapySessions = therapyAppointments.Count,
                PendingTherapySessions = therapyAppointments.Count(a => a.Status == "Pending"),
                ApprovedTherapySessions = therapyAppointments.Count(a => a.Status == "Approved"),
                CompletedTherapySessions = therapyAppointments.Count(a => a.Status == "Completed"),
                CancelledTherapySessions = therapyAppointments.Count(a => a.Status == "Cancelled"),

                // Psychiatry statistics
                TotalPsychiatrySessions = psychiatryAppointments.Count,
                PendingPsychiatrySessions = psychiatryAppointments.Count(a => a.Status == "Pending"),
                ApprovedPsychiatrySessions = psychiatryAppointments.Count(a => a.Status == "Approved"),
                CompletedPsychiatrySessions = psychiatryAppointments.Count(a => a.Status == "Completed"),
                CancelledPsychiatrySessions = psychiatryAppointments.Count(a => a.Status == "Cancelled"),

                // Upcoming appointments (both therapy and psychiatry)
                UpcomingTherapyAppointments = therapyAppointments
                    .Where(a => a.Status == "Pending" || a.Status == "Approved")
                    .Where(a => a.StartTime > DateTime.UtcNow)
                    .OrderBy(a => a.StartTime)
                    .ToList(),

                UpcomingPsychiatryAppointments = psychiatryAppointments
                    .Where(a => a.Status == "Pending" || a.Status == "Approved")
                    .Where(a => a.StartTime > DateTime.UtcNow)
                    .OrderBy(a => a.StartTime)
                    .ToList(),

                // Recent appointments (both therapy and psychiatry)
                RecentTherapyAppointments = therapyAppointments
                    .Where(a => a.Status == "Completed")
                    .OrderByDescending(a => a.StartTime)
                    .Take(5)
                    .ToList(),

                RecentPsychiatryAppointments = psychiatryAppointments
                    .Where(a => a.Status == "Completed")
                    .OrderByDescending(a => a.StartTime)
                    .Take(5)
                    .ToList(),

                // All prescriptions
                Prescriptions = prescriptions,

                // Active prescriptions
                ActivePrescriptions = prescriptions
                    .Where(p => p.Status == "Active")
                    .ToList(),

                // Statistics for medications
                TotalPrescriptions = prescriptions.Count,
                ActivePrescriptionsCount = prescriptions.Count(p => p.Status == "Active"),
                InactivePrescriptionsCount = prescriptions.Count(p => p.Status == "Inactive")
            };

            // List of therapists and psychiatrists the student has appointments with
            var therapistIds = therapyAppointments.Select(a => a.TherapistId).Distinct().ToList();
            var psychiatristIds = psychiatryAppointments.Select(a => a.PsychiatristId).Distinct().ToList();

            var therapists = await _userManager.Users
                .Where(u => therapistIds.Contains(u.Id))
                .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName}" })
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            var psychiatrists = await _userManager.Users
                .Where(u => psychiatristIds.Contains(u.Id))
                .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName}" })
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            reportViewModel.TherapistNames = therapists;
            reportViewModel.PsychiatristNames = psychiatrists;

            // Generate report date
            reportViewModel.ReportGeneratedDate = DateTime.UtcNow;

            return View(reportViewModel);
        }

        // Add PDF export action method
        public async Task<IActionResult> ExportPdf()
        {
            // This is a placeholder for PDF export functionality
            // You'd need to implement a PDF generator here
            // For example, using a library like Rotativa, iTextSharp, or DinkToPdf

            TempData["Message"] = "PDF export feature is coming soon.";
            return RedirectToAction(nameof(Index));
        }
    }
}