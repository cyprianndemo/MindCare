using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using System.Security.Claims;

namespace MindCare.Controllers
{
    [Authorize]
    public class MoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MoodController> _logger;

        public MoodController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<MoodController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: MoodEntry
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var moodEntries = await _context.MoodEntries
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.EntryDate)
                .ToListAsync();
            return View(moodEntries);
        }

        // GET: MoodEntry/Create
        public IActionResult Create()
        {
            return View(new MoodEntry { EntryDate = DateTime.UtcNow });
        }

        // POST: MoodEntry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MoodEntry moodEntry)
        {
            try
            {
                // Remove these fields from validation since we'll set them manually
                ModelState.Remove("UserId");
                ModelState.Remove("User");
                ModelState.Remove("EntryDate");

                if (ModelState.IsValid)
                {
                    // Get current user ID
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogError("User ID is null or empty");
                        throw new Exception("User not found");
                    }

                    // Create a new MoodEntry instance to avoid tracking issues
                    var newMoodEntry = new MoodEntry
                    {
                        Mood = moodEntry.Mood,
                        Intensity = moodEntry.Intensity,
                        Notes = moodEntry.Notes,
                        UserId = userId,
                        EntryDate = DateTime.UtcNow
                    };

                    _logger.LogInformation($"Attempting to save mood entry: Mood={newMoodEntry.Mood}, Intensity={newMoodEntry.Intensity}, UserId={newMoodEntry.UserId}");

                    // Add to context
                    await _context.MoodEntries.AddAsync(newMoodEntry);

                    // Save changes
                    var saveResult = await _context.SaveChangesAsync();
                    _logger.LogInformation($"SaveChanges result: {saveResult}");

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogError($"ModelState errors: {string.Join(", ", errors)}");
                    return View(moodEntry);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error: {ex.Message}");
                _logger.LogError($"Inner exception: {ex.InnerException?.Message}");
                ModelState.AddModelError("", $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving mood entry: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(moodEntry);
        }

        // GET: MoodEntry/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var moodEntry = await _context.MoodEntries
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (moodEntry == null)
            {
                return NotFound();
            }

            return View(moodEntry);
        }
        public async Task<IActionResult> Analytics()
        {
            var moodAnalytics = await _context.MoodEntries
                .GroupBy(m => m.Mood)
                .Select(g => new MoodAnalytics
                {
                    Mood = g.Key,
                    Count = g.Count(),
                    AverageIntensity = g.Average(m => m.Intensity)
                })
                .ToListAsync();

            return View(moodAnalytics);
        }
    }
}