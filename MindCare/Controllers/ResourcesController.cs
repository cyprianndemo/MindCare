using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.ViewModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MindCare.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResourcesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string category = null, string type = null, string scenario = null, string searchTerm = null)
        {
            var resources = _context.MentalHealthResources.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(category))
                resources = resources.Where(r => r.Category == category);

            if (!string.IsNullOrEmpty(type))
                resources = resources.Where(r => r.ResourceType == type);

            if (!string.IsNullOrEmpty(scenario))
                resources = resources.Where(r => r.Scenario == scenario);

            // Search functionality
            if (!string.IsNullOrEmpty(searchTerm))
            {
                resources = resources.Where(r =>
                    r.Title.Contains(searchTerm) ||
                    r.Description.Contains(searchTerm) ||
                    r.Content.Contains(searchTerm) ||
                    r.Tags.Any(t => t.Contains(searchTerm))
                );
            }

            // Get recommended resources
            var recommendedResources = await _context.MentalHealthResources
                .Where(r => r.Category == category || category == null)
                .OrderByDescending(r => r.DateAdded)
                .Take(3)
                .ToListAsync();

            var viewModel = new ResourceViewModel
            {
                Resources = await resources.ToListAsync(),
                RecommendedResources = recommendedResources,
                SelectedCategory = category,
                SelectedType = type,
                SelectedScenario = scenario,
                SearchTerm = searchTerm,
                Categories = ResourceConstants.Categories.Keys.ToList(),
                ResourceTypes = ResourceConstants.ResourceTypes.Keys.ToList(),
                Scenarios = ResourceConstants.Scenarios.Keys.ToList(),
                ScenarioDescriptions = ResourceConstants.Scenarios
            };

            return View(viewModel);
        }

        /*public async Task<IActionResult> Read(int id)
        {
            var resource = await _context.MentalHealthResources.FindAsync(id);
            if (resource == null)
                return NotFound();

            // Get related resources
            var relatedResources = await _context.MentalHealthResources
                .Where(r => r.Category == resource.Category && r.Id != resource.Id)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedResources = relatedResources;
            return View(resource);
        }*/

        public async Task<IActionResult> Details(int id)
        {
            var resource = await _context.MentalHealthResources.FindAsync(id);
            if (resource == null)
                return NotFound();

            return View(resource);
        }

        public async Task<IActionResult> GetResourcesByCategory(string category)
        {
            var resources = await _context.MentalHealthResources
                .Where(r => r.Category == category)
                .ToListAsync();

            return Json(resources);
        }

        public async Task<IActionResult> Crisis()
        {
            var emergencyResources = await _context.MentalHealthResources
                .Where(r => r.Urgency == "Immediate" && r.Category == "Crisis")
                .ToListAsync();

            return View(emergencyResources);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string term)
        {
            var results = await _context.MentalHealthResources
                .Where(r =>
                    r.Title.Contains(term) ||
                    r.Description.Contains(term) ||
                    r.Content.Contains(term) ||
                    r.Tags.Any(t => t.Contains(term)))
                .Take(5)
                .ToListAsync();

            return Json(results);
        }
        // In ResourcesController.cs - enhance the Read action
        public async Task<IActionResult> Read(int id)
        {
            var resource = await _context.MentalHealthResources.FindAsync(id);
            if (resource == null)
                return NotFound();

            // Track this view to improve recommendations
            await TrackResourceView(resource);

            // Get related resources based on category and tags
            var relatedResources = await _context.MentalHealthResources
                .Where(r => (r.Category == resource.Category ||
                            r.Tags.Any(t => resource.Tags.Contains(t))) &&
                            r.Id != resource.Id)
                .Take(5)
                .ToListAsync();

            // Get interactive exercises related to this resource
            var relatedExercises = await _context.MentalHealthExercises
                .Where(e => e.RelatedResourceCategories.Contains(resource.Category))
                .Take(3)
                .ToListAsync();

            // Get professional resources related to this topic
            var professionalResources = await _context.ProfessionalResources
                .Where(p => p.Categories.Contains(resource.Category))
                .Take(3)
                .ToListAsync();

            var viewModel = new ResourceReadViewModel
            {
                Resource = resource,
                RelatedResources = relatedResources,
                RelatedExercises = relatedExercises,
                ProfessionalResources = professionalResources
            };

            return View(viewModel);
        }

        private async Task TrackResourceView(MentalHealthResource resource)
        {
            resource.ViewCount++;
            _context.Update(resource);
            await _context.SaveChangesAsync();
        }
        public async Task<IActionResult> Exercise(int id)
        {
            var exercise = await _context.MentalHealthExercises.FindAsync(id);
            if (exercise == null)
                return NotFound();

            return View(exercise);
        }

        public async Task<IActionResult> Toolbox()
        {
            var tools = await _context.MentalHealthResources
                .Where(r => r.ResourceType == "Tool" || r.ResourceType == "Exercise")
                .OrderBy(r => r.Category)
                .ToListAsync();

            return View(tools);
        }

        // Add an Assessment action for mental health assessments
        public async Task<IActionResult> Assessment()
        {
            var assessments = await _context.MentalHealthAssessments
                .OrderBy(a => a.Order)
                .ToListAsync();

            return View(assessments);
        }

        // Add a Professionals action to find professional help
        public async Task<IActionResult> Professionals()
        {
            var professionals = await _context.ProfessionalResources
                .OrderBy(p => p.Category)
                .ToListAsync();

            return View(professionals);
        }
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            // Get the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "You need to be logged in to favorite resources" });

            // Check if this resource is already favorited
            var favorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ResourceId == id);

            if (favorite == null)
            {
                // Add as favorite
                _context.UserFavorites.Add(new UserFavorite
                {
                    UserId = userId,
                    ResourceId = id,
                    DateAdded = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Added to your favorites" });
            }
            else
            {
                // Remove from favorites
                _context.UserFavorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Removed from your favorites" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int resourceId, int progress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            var userProgress = await _context.UserProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ResourceId == resourceId);

            if (userProgress == null)
            {
                _context.UserProgress.Add(new UserProgress
                {
                    UserId = userId,
                    ResourceId = resourceId,
                    ProgressPercentage = progress,
                    LastUpdated = DateTime.Now
                });
            }
            else
            {
                userProgress.ProgressPercentage = progress;
                userProgress.LastUpdated = DateTime.Now;
                _context.Update(userProgress);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}