using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MindCare.Data;
using MindCare.Models;
using Microsoft.AspNetCore.Identity;

namespace MindCare.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get appointments for the current user with their therapists
            var appointments = await _context.Appointments
                .Include(a => a.Therapist)
                .Where(a => a.StudentId == userId)
                .Select(a => new
                {
                    TherapistId = a.TherapistId,
                    TherapistName = $"{a.Therapist.FirstName} {a.Therapist.LastName}"
                })
                .Distinct()
                .ToListAsync();

            ViewBag.Therapists = new SelectList(appointments, "TherapistId", "TherapistName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.StudentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                feedback.CreatedAt = DateTime.UtcNow;

                _context.Feedback.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction("Thanks");
            }

            // If we got here, something failed, redisplay form
            var appointments = await _context.Appointments
                .Include(a => a.Therapist)
                .Where(a => a.StudentId == feedback.StudentId)
                .Select(a => new
                {
                    TherapistId = a.TherapistId,
                    TherapistName = $"{a.Therapist.FirstName} {a.Therapist.LastName}"
                })
                .Distinct()
                .ToListAsync();

            ViewBag.Therapists = new SelectList(appointments, "TherapistId", "TherapistName");
            return View(feedback);
        }

        // Action for therapists to view feedback

        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> ViewFeedbacks()
        {
            var therapistId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var feedbacks = await _context.Feedback
                .Include(f => f.Student)
                .Where(f => f.TherapistId == therapistId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(feedbacks);
        }
        [Authorize(Roles = "Psychiatrist")]
        public async Task<IActionResult> ViewFeedback()
        {
            var therapistId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var feedbacks = await _context.Feedback
                .Include(f => f.Student)
                .Where(f => f.TherapistId == therapistId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(feedbacks);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedback.FindAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            // Optional: Add security check to ensure this therapist owns this feedback
            

            try
            {
                _context.Feedback.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback deleted successfully.";
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new { message = "An error occurred while deleting the feedback" });
            }

            return RedirectToAction(nameof(Feedback));
        }
        public IActionResult Thanks()
        {
            return View();
        }
    }
}