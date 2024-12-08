using Microsoft.AspNetCore.Mvc;
using MindCare.Data;
using MindCare.Models;

namespace MindCare.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult PatientFeedback()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Feedback.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction("Thanks"); // Redirect to a thank-you page
            }
            return View(feedback);
        }

        public IActionResult Thanks()
        {
            return View();
        }
    }
}
