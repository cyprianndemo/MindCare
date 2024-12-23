using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using MindCare.ViewModel;

namespace MindCare.Controllers
{
    public class PharmacyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PharmacyController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> PrescriptionList()
        {
            var prescriptions = await _context.Prescriptions.ToListAsync();
            return View(prescriptions);
        }

        public async Task<IActionResult> MedicationList(int prescriptionId)
        {
            var medications = await _context.Medications
                .Where(m => m.Prescriptions.Any(p => p.PrescriptionId == prescriptionId))
                .ToListAsync();
            return View(medications);
        }

        // GET: Pharmacy/AddMedication
        public IActionResult AddMedication()
        {
            return View();
        }

        // POST: Pharmacy/AddMedication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedication(MedicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                if (model.MedicationImage != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/medications");
                    
                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.MedicationImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.MedicationImage.CopyToAsync(fileStream);
                    }
                }

                var medication = new Medication
                {
                    Name = model.Name,
                    Description = model.Description,
                    Dosage = model.Dosage,
                    ImagePath = uniqueFileName,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.Medications.Add(medication);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Medication added successfully!";
                return RedirectToAction(nameof(MedicationList));
            }

            return View(model);
        }

        public IActionResult PrescribeMedication()
        {
            return View();
        }

        public IActionResult Prescriptions()
        {
            return View();
        }
    }
}