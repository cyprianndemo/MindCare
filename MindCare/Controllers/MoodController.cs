using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MindCare.Controllers
{
    public class MoodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Predefined list of moods
        private List<string> GetPredefinedMoods()
        {
            return new List<string>
            {
                "Happy",
                "Excited",
                "Calm",
                "Content",
                "Neutral",
                "Anxious",
                "Sad",
                "Angry",
                "Stressed",
                "Overwhelmed"
            };
        }

        // View Mood History
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var moods = await _context.MoodEntries
                .Where(m => m.StudentId == userId)
                .OrderByDescending(m => m.EntryDate)
                .ToListAsync();

            return View(moods);
        }

        // Add a new Mood Entry
        [HttpGet]
        public IActionResult AddMood()
        {
            ViewBag.PredefinedMoods = GetPredefinedMoods();
            ViewBag.IntensityLevels = Enumerable.Range(1, 10).Select(i => new
            {
                Value = i,
                Description = $"{i} - {GetIntensityDescription(i)}"
            });
            return View();
        }

        private string GetIntensityDescription(int level)
        {
            return level switch
            {
                1 => "Very Low",
                2 => "Low",
                3 => "Somewhat Low",
                4 => "Below Average",
                5 => "Moderate",
                6 => "Above Average",
                7 => "High",
                8 => "Very High",
                9 => "Extremely High",
                10 => "Maximum",
                _ => ""
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMood(MoodEntry moodEntry)
        {
            if (ModelState.IsValid)
            {
                moodEntry.StudentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                moodEntry.EntryDate = DateTime.Now;
                _context.Add(moodEntry);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Mood entry saved successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PredefinedMoods = GetPredefinedMoods();
            ViewBag.IntensityLevels = Enumerable.Range(1, 10).Select(i => new
            {
                Value = i,
                Description = $"{i} - {GetIntensityDescription(i)}"
            });
            return View(moodEntry);
        }

        // Analytics
        public async Task<IActionResult> Analytics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var moodData = await _context.MoodEntries
                .Where(m => m.StudentId == userId)
                .GroupBy(m => m.Mood)
                .Select(group => new
                {
                    Mood = group.Key,
                    Count = group.Count()
                })
                .ToListAsync();

            return View(moodData);
        }
    }
}