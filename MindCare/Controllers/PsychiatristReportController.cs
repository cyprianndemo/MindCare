using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.PsychiatristReport;
using MindCare.ViewModel;

namespace MindCare.Controllers
{
    [Authorize(Roles = "Psychiatrist")]
    public class PsychiatristReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PsychiatristReportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Report()
        {
            // Get the currently logged in psychiatrist
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Get psychiatrist's full name
            string psychiatristName = $"{currentUser.FirstName} {currentUser.LastName}";

            // Query for appointments
            var appointments = await _context.Appointments
                .Where(a => a.PsychiatristId == currentUser.Id)
                .ToListAsync();

            // Calculate statistics
            int totalSessions = appointments.Count;
            int pendingSessions = appointments.Count(a => a.Status == "Pending");
            int approvedSessions = appointments.Count(a => a.Status == "Approved");

            // Get prescribed medications
            var medications = await _context.Prescriptions
                .Include(p => p.Student)
                .Where(p => p.PsychiatristId == currentUser.Id)
                .Select(p => new PrescriptionViewModel
                {
                    StudentName = $"{p.Student.FirstName} {p.Student.LastName}",
                    MedicationName = p.Medication != null ? p.Medication.Name : "Unknown",
                    Dosage = p.Dosage,
                    PrescriptionDate = p.PrescribedDate,
                    Instructions = p.Instructions
                })
                .ToListAsync();

            // Create view model
            var viewModel = new PsychiatristReportViewModel
            {
                PsychiatristName = psychiatristName,
                TotalSessions = totalSessions,
                PendingSessions = pendingSessions,
                ApprovedSessions = approvedSessions,
                Prescriptions = medications,
                GeneratedDate = DateTime.Now
            };

            return View(viewModel);
        }
    }
}
