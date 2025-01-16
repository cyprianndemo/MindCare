using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace MindCare.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _converter;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResourcesController(
            ApplicationDbContext context,
            IConverter converter,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _converter = converter;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string scenario = "", string category = "", string resourceType = "")
        {
            var query = _context.MentalHealthResources.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(scenario))
            {
                query = query.Where(r => r.Scenario == scenario);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.Category == category);
            }

            if (!string.IsNullOrEmpty(resourceType))
            {
                query = query.Where(r => r.ResourceType == resourceType);
            }

            var resources = await query
                .OrderByDescending(r => r.Urgency == "Immediate")
                .ThenByDescending(r => r.DateAdded)
                .ToListAsync();

            var viewModel = new ResourceViewModel
            {
                Resources = resources,
                SelectedScenario = scenario,
                SelectedCategory = category,
                SelectedType = resourceType,
                Categories = ResourceConstants.Categories.Keys.ToList(),
                ResourceTypes = ResourceConstants.ResourceTypes.Keys.ToList(),
                Scenarios = ResourceConstants.Scenarios.Keys.ToList(),
                ScenarioDescriptions = ResourceConstants.Scenarios
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Download(int id)
        {
            var resource = await _context.MentalHealthResources.FindAsync(id);

            if (resource == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(resource.FileUrl))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "resources", resource.FileUrl);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, GetContentType(resource.FileUrl), resource.FileUrl);
            }

            // Generate PDF for online resources
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects = {
                    new ObjectSettings()
                    {
                        PagesCount = true,
                        HtmlContent = GeneratePdfHtml(resource),
                        WebSettings = { DefaultEncoding = "utf-8" },
                    }
                }
            };

            var pdf = _converter.Convert(doc);
            return File(pdf, "application/pdf", $"{resource.Title}.pdf");
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        private string GeneratePdfHtml(MentalHealthResource resource)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; padding: 20px; }}
                        h1 {{ color: #2c3e50; }}
                        .content {{ line-height: 1.6; color: #34495e; }}
                        .tags {{ color: #7f8c8d; font-size: 0.9em; }}
                    </style>
                </head>
                <body>
                    <h1>{resource.Title}</h1>
                    <div class='tags'>
                        Category: {resource.Category} | Type: {resource.ResourceType} | Scenario: {resource.Scenario}
                    </div>
                    <div class='content'>
                        <h2>Description</h2>
                        {resource.Description}
                        <h2>Content</h2>
                        {resource.Content}
                    </div>
                </body>
                </html>";
        }
    }
}