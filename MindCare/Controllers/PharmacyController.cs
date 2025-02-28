using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using MindCare.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Medication)
                .Include(p => p.Student)
                .Include(p => p.Psychiatrist)
                .Select(p => new PrescriptionViewModel
                {
                    PrescriptionId = p.PrescriptionId,
                    MedicationId = p.MedicationId,
                    StudentId = p.StudentId,
                    PsychiatristId = p.PsychiatristId,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    Duration = p.Duration,
                    Instructions = p.Instructions,
                    PrescriptionDate = p.PrescriptionDate,
                    Status = p.Status,
                    Student = p.Student,
                    Psychiatrist = p.Psychiatrist,
                    Medication = p.Medication,
                    MedicationName = p.Medication.Name
                })
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            return View(prescriptions);
        }

        // GET: Student's Prescriptions
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyPrescriptions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var prescriptions = await _context.Prescriptions
                .Where(p => p.Student.Email == userEmail) // Ensure email matches the student
                .Select(p => new PrescriptionViewModel
                {
                    PrescriptionId = p.PrescriptionId,
                    MedicationName = p.Medication.Name,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    PrescriptionDate = p.PrescriptionDate,
                    Status = p.Status
                })
                .ToListAsync();

            return View(prescriptions);
        }


        // GET: Medication Details for a specific student
        [Authorize]
        public async Task<IActionResult> StudentMedicationDetails(int prescriptionId)
        {
            // Get current user's ID and role
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Query to get the prescription with all related data
            var prescriptionQuery = _context.Prescriptions
                .Include(p => p.Medication)
                .Include(p => p.Student)
                .Include(p => p.Psychiatrist)
                .AsQueryable();

            // If student, only show their own prescriptions
            if (userRole == "Student")
            {
                prescriptionQuery = prescriptionQuery.Where(p => p.StudentId == userId);
            }
            // If psychiatrist, only show prescriptions they created
            else if (userRole == "Psychiatrist")
            {
                prescriptionQuery = prescriptionQuery.Where(p => p.PsychiatristId == userId);
            }

            var prescription = await prescriptionQuery
                .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

            if (prescription == null)
            {
                TempData["Error"] = "Prescription not found or you don't have access to view it.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new PrescriptionViewModel
            {
                PrescriptionId = prescription.PrescriptionId,
                MedicationId = prescription.MedicationId,
                StudentId = prescription.StudentId,
                PsychiatristId = prescription.PsychiatristId,
                Dosage = prescription.Dosage,
                Frequency = prescription.Frequency,
                Duration = prescription.Duration,
                Instructions = prescription.Instructions,
                PrescriptionDate = prescription.PrescriptionDate,
                Status = prescription.Status,
                Student = prescription.Student,
                Psychiatrist = prescription.Psychiatrist,
                Medication = prescription.Medication,
                MedicationName = prescription.Medication.Name
            };

            return View(viewModel);
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

        [Authorize(Roles = "Psychiatrist")]
        public async Task<IActionResult> PrescribeMedication(int id)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(id);
                if (medication == null)
                {
                    return NotFound();
                }

                var psychiatristId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get students assigned to the psychiatrist
                var patients = await _context.Appointments
                    .Where(a => a.PsychiatristId == psychiatristId)
                    .Select(a => a.Student)
                    .Distinct()
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id,
                        Text = $"{s.FirstName} {s.LastName} ({s.Email})"
                    })
                    .ToListAsync();

                var viewModel = new PrescriptionViewModel
                {
                    MedicationId = medication.MedicationId,
                    MedicationName = medication.Name,
                    Patients = patients,
                    PrescriptionDate = DateTime.Today
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the prescription form.";
                return RedirectToAction("MedicationList");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Psychiatrist")]
        public async Task<IActionResult> PrescribeMedication(PrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Patients = await GetPsychiatristPatients(User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["Error"] = "Please fill in all required fields correctly.";
                return View(model);
            }

            try
            {
                var psychiatristId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Ensure valid patient selection
                var isValidPatient = await _context.Appointments
                    .AnyAsync(a => a.PsychiatristId == psychiatristId && a.StudentId == model.StudentId);

                if (!isValidPatient)
                {
                    TempData["Error"] = "Invalid patient selection. Please select a valid patient.";
                    model.Patients = await GetPsychiatristPatients(psychiatristId);
                    return View(model);
                }

                var medication = await _context.Medications.FindAsync(model.MedicationId);
                if (medication == null)
                {
                    TempData["Error"] = "Selected medication not found.";
                    return RedirectToAction(nameof(MedicationList));
                }

                // Check for existing prescription
                var existingPrescription = await _context.Prescriptions
                    .AnyAsync(p => p.MedicationId == model.MedicationId && p.StudentId == model.StudentId && p.Status == "Active");

                if (existingPrescription)
                {
                    TempData["Warning"] = "This medication is already prescribed to this student.";
                    model.Patients = await GetPsychiatristPatients(psychiatristId);
                    return View(model);
                }

                // Save new prescription
                var prescription = new Prescription
                {
                    MedicationId = model.MedicationId,
                    StudentId = model.StudentId,
                    PsychiatristId = psychiatristId,
                    Dosage = model.Dosage,
                    Frequency = model.Frequency,
                    Duration = model.Duration,
                    Instructions = model.Instructions,
                    PrescriptionDate = model.PrescriptionDate,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                // Create notification for student
                var notification = new Notification
                {
                    UserId = model.StudentId,
                    Message = $"New medication prescribed: {medication.Name} - {model.Dosage}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Type = "Prescription",
                    Link = $"/Pharmacy/StudentMedicationDetails/{prescription.PrescriptionId}"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Medication prescribed successfully!";
                return RedirectToAction(nameof(PrescriptionList));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while prescribing the medication.");
                TempData["Error"] = "Failed to prescribe medication. Please try again.";
                model.Patients = await GetPsychiatristPatients(User.FindFirstValue(ClaimTypes.NameIdentifier));
                return View(model);
            }
        }


        // Helper method to get psychiatrist's patients
        private async Task<List<SelectListItem>> GetPsychiatristPatients(string psychiatristId)
        {
            return await _context.Appointments
                .Where(a => a.PsychiatristId == psychiatristId)
                .Join(
                    _context.Users,
                    appointment => appointment.StudentId,
                    user => user.Id,
                    (appointment, user) => new { User = user }
                )
                .Distinct()
                .Select(x => new SelectListItem
                {
                    Value = x.User.Id,
                    Text = $"{x.User.FirstName} {x.User.LastName} ({x.User.Email})"
                })
                .ToListAsync();
        }

        [Authorize]
        public async Task<IActionResult> Prescriptions()
        {
            // For student role: Get their own prescriptions
            // For psychiatrist: Get all prescriptions they created
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var prescriptionsQuery = _context.Prescriptions
                .Include(p => p.Medication)
                .Include(p => p.Student)
                .Include(p => p.Psychiatrist)
                .AsQueryable();

            if (userRole == "Student")
            {
                prescriptionsQuery = prescriptionsQuery.Where(p => p.StudentId == userId);
            }
            else if (userRole == "Psychiatrist")
            {
                prescriptionsQuery = prescriptionsQuery.Where(p => p.PsychiatristId == userId);
            }

            var prescriptions = await prescriptionsQuery
                .OrderByDescending(p => p.PrescriptionDate)
                .Select(p => new PrescriptionViewModel
                {
                    PrescriptionId = p.PrescriptionId,
                    MedicationId = p.MedicationId,
                    StudentId = p.StudentId,
                    PsychiatristId = p.PsychiatristId,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    Duration = p.Duration,
                    Instructions = p.Instructions,
                    PrescriptionDate = p.PrescriptionDate,
                    Status = p.Status,
                    Student = p.Student,
                    Psychiatrist = p.Psychiatrist,
                    Medication = p.Medication,
                    MedicationName = p.Medication.Name
                })
                .ToListAsync();

            ViewBag.UserRole = userRole;
            return View(prescriptions);
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
                ExistingImagePath = medication.ImagePath
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