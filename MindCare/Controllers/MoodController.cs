using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCare.Data;
using MindCare.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MindCare.Controllers
{
    [Authorize]
    public class MoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MoodController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userMoods = _context.MoodEntries
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.EntryDate)
                .ToList();
            return View(userMoods);
        }

        // GET: Mood/AddMood
        public IActionResult AddMood()
        {
            SetViewBagData();
            return View();
        }

        // POST: Mood/AddMood
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMood(MoodEntry moodEntry)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the current user's ID
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "User not found. Please try logging in again.");
                        SetViewBagData();
                        return View(moodEntry);
                    }

                    moodEntry.UserId = userId;
                    moodEntry.EntryDate = DateTime.UtcNow;

                    await _context.MoodEntries.AddAsync(moodEntry);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Mood entry added successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Log the exception here
                ModelState.AddModelError("", "An error occurred while saving the mood entry.");
            }

            SetViewBagData();
            return View(moodEntry);
        }

        private void SetViewBagData()
        {
            ViewBag.PredefinedMoods = new[] { "Happy", "Sad", "Angry", "Excited", "Anxious", "Neutral" };
            ViewBag.IntensityLevels = Enumerable.Range(1, 10)
                .Select(i => new
                {
                    Value = i,
                    Description = $"Level {i} - {GetIntensityDescription(i)}"
                });
        }

        private string GetIntensityDescription(int level)
        {
            return level switch
            {
                1 => "Very Mild",
                2 => "Mild",
                3 => "Somewhat Mild",
                4 => "Moderate-Low",
                5 => "Moderate",
                6 => "Moderate-High",
                7 => "Somewhat Strong",
                8 => "Strong",
                9 => "Very Strong",
                10 => "Extreme",
                _ => string.Empty
            };
        }

        // GET: Mood/Analytics
        public IActionResult Analytics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var moodData = _context.MoodEntries
                .Where(m => m.UserId == userId)
                .GroupBy(m => m.Mood)
                .Select(g => new MoodAnalytics
                {
                    Mood = g.Key,
                    Count = g.Count(),
                    AverageIntensity = Math.Round(g.Average(m => m.Intensity), 1)
                })
                .ToList();

            return View(moodData);
        }
    }
}