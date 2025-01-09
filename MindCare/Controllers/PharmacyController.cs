using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using MindCare.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        // Updated MedicationList action to show all medications
        public async Task<IActionResult> MedicationList()
        {
            try
            {
                // Fetch all medications from the database
                var medications = await _context.Medications
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                if (medications == null || !medications.Any())
                {
                    TempData["Info"] = "No medications found in the database.";
                    return View(new List<Medication>());
                }

                return View(medications);
            }
            catch (Exception ex)
            {
                // Log the error here if you have logging configured
                TempData["Error"] = "An error occurred while retrieving medications.";
                return View(new List<Medication>());
            }
        }

        // Get medication by ID
        public async Task<IActionResult> MedicationDetails(int id)
        {
            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.MedicationId == id);

            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // GET: Pharmacy/AddMedication
        public IActionResult AddMedication()
        {
            return View(new MedicationViewModel());
        }

        // POST: Pharmacy/AddMedication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedication(MedicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = null;

                    if (model.MedicationImage != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "medications");
                        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

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
                        Price = model.Price,
                        ImagePath = uniqueFileName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };

                    _context.Medications.Add(medication);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Medication added successfully!";
                    return RedirectToAction(nameof(MedicationList));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error occurred while saving the medication. Please try again.");
                    return View(model);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> PrescribeMedication(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }

            // Get list of patients
            var patients = await _context.Students
                .Select(p => new SelectListItem
                {
                    Value = p.StudentId.ToString(),
                    Text = $"{p.FirstName} {p.LastName} ({p.StudentId})"
                })
                .ToListAsync();

            var viewModel = new PrescriptionViewModel
            {
                MedicationId = medication.MedicationId,
                MedicationName = medication.Name,
                Patients = patients
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrescribeMedication(PrescriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var prescription = new Prescription
                {
                    MedicationId = model.MedicationId,
                    StudentId = model.StudentId,
                    Dosage = model.Dosage,
                    Frequency = model.Frequency,
                    Duration = model.Duration,
                    Instructions = model.Instructions,
                    PrescriptionDate = model.PrescriptionDate,
                    Status = "Active"
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Medication prescribed successfully!";
                return RedirectToAction(nameof(PrescriptionList));
            }

            // If we got this far, something failed, redisplay form
            model.Patients = await _context.Students
                .Select(p => new SelectListItem
                {
                    Value = p.StudentId.ToString(),
                    Text = $"{p.FirstName} {p.LastName} ({p.StudentId})"
                })
                .ToListAsync();

            return View(model);
        }

        public IActionResult Prescriptions()
        {
            return View();
        }
        public IActionResult MedicationDetails()
        {
            return View();
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }

            var viewModel = new MedicationViewModel
            {
                MedicationId = medication.MedicationId,
                Name = medication.Name,
                Description = medication.Description,
                Dosage = medication.Dosage,
                Price = medication.Price,
               // ExistingImagePath = medication.ImagePath
            };

            return View(viewModel);
        }

        // POST: Edit Medication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicationViewModel model)
        {
            if (id != model.MedicationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var medication = await _context.Medications.FindAsync(id);
                    if (medication == null)
                    {
                        return NotFound();
                    }

                    // Handle new image upload
                    if (model.MedicationImage != null)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(medication.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images/medications", medication.ImagePath);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/medications");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.MedicationImage.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.MedicationImage.CopyToAsync(fileStream);
                        }

                        medication.ImagePath = uniqueFileName;
                    }

                    // Update other properties
                    medication.Name = model.Name;
                    medication.Description = model.Description;
                    medication.Dosage = model.Dosage;
                    medication.Price = model.Price;
                    medication.UpdatedDate = DateTime.UtcNow;

                    _context.Update(medication);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Medication updated successfully!";
                    return RedirectToAction(nameof(MedicationList));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        // POST: Delete Medication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(id);
                if (medication == null)
                {
                    return Json(new { success = false, message = "Medication not found." });
                }

                // Delete the image file if it exists
                if (!string.IsNullOrEmpty(medication.ImagePath))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images/medications", medication.ImagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Medications.Remove(medication);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Medication deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting medication." });
            }
        }

        private bool MedicationExists(int id)
        {
            return _context.Medications.Any(e => e.MedicationId == id);
        }
    }
}