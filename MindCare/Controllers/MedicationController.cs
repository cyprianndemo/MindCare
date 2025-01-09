/*using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MindCare.Models;
using System.Linq;
using System.Threading.Tasks;
using MindCare.Data;
using System;

namespace MindCare.Controllers
{
    public class MedicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedicationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> PrescribeMedication(string studentEmail)
        {
            // Retrieve the logged-in psychiatrist
            var user = await _userManager.GetUserAsync(User);
            var isPsychiatrist = await _userManager.IsInRoleAsync(user, "Psychiatrist");

            if (!isPsychiatrist)
            {
                TempData["Error"] = "You are not authorized to prescribe medication.";
                return RedirectToAction("Index", "Home");
            }

            // Ensure the student email is provided and valid
            if (string.IsNullOrEmpty(studentEmail))
            {
                TempData["Error"] = "No student email provided for prescribing medication.";
                return RedirectToAction("Index", "Home");
            }

            var student = await _userManager.FindByEmailAsync(studentEmail);
            if (student == null || !await _userManager.IsInRoleAsync(student, "Student"))
            {
                TempData["Error"] = "Invalid student email or the user is not a student.";
                return RedirectToAction("Index", "Home");
            }

            // Get all medications from the database
            var medications = _context.Medications.ToList();
            ViewBag.Medications = medications;
            ViewBag.StudentEmail = studentEmail;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PrescribeMedication(string studentEmail, int medicationId, string instructions)
        {
            // Retrieve the logged-in psychiatrist
            var user = await _userManager.GetUserAsync(User);
            var isPsychiatrist = await _userManager.IsInRoleAsync(user, "Psychiatrist");

            if (!isPsychiatrist)
            {
                TempData["Error"] = "You are not authorized to prescribe medication.";
                return RedirectToAction("Index", "Home");
            }

            // Validate the student email
            var student = await _userManager.FindByEmailAsync(studentEmail);
            if (student == null || !await _userManager.IsInRoleAsync(student, "Student"))
            {
                TempData["Error"] = "Invalid student email or the user is not a student.";
                return RedirectToAction("Index", "Home");
            }

            // Create the prescription
            var prescription = new Prescription
            {
                MedicationId = medicationId,
                StudentId = student.Id, // Use student's unique ID
                PsychiatristId = user.Id, // Psychiatrist's unique ID
                Instructions = instructions,
                PrescribedDate = DateTime.Now
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Medication prescribed successfully.";
            return RedirectToAction("Prescriptions");
        }

        public async Task<IActionResult> Prescriptions()
        {
            // Retrieve the logged-in psychiatrist
            var user = await _userManager.GetUserAsync(User);
            var isPsychiatrist = await _userManager.IsInRoleAsync(user, "Psychiatrist");

            if (!isPsychiatrist)
            {
                TempData["Error"] = "You are not authorized to view prescriptions.";
                return RedirectToAction("Index", "Home");
            }

            // Get all prescriptions made by this psychiatrist
            var prescriptions = _context.Prescriptions
                .Where(p => p.PsychiatristId == user.Id)
                .Select(p => new
                {
                    p.PrescriptionId,
                    MedicationName = p.Medication.Name,
                    p.StudentId,
                    p.Instructions,
                    p.PrescribedDate
                })
                .ToList();

            return View(prescriptions);
        }
    }
}
*/