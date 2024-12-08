using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.ViewModel;
using System.Security.Claims;

namespace MindCare.Controllers
{
    public class TherapistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public TherapistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View("Index");
        }

        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> ManageSessions()
        {
            try
            {
                // Get the logged-in Therapist's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var Therapist = await _userManager.FindByIdAsync(userId);

                if (Therapist == null)
                {
                    TempData["Error"] = "Therapist not found.";
                    return RedirectToAction("Dashboard");
                }

                // Fetch pending appointments with related data
                var appointments = await _context.Appointments
                    .Include(a => a.Student)
                    .Include(a => a.Therapist)
                    .Where(a => a.TherapistId == Therapist.Id && a.Status == "Pending")
                    .Select(a => new Appointment
                    {
                        AppointmentId = a.AppointmentId,
                        Date = a.Date,
                        Time = a.Time,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        TherapistId = a.TherapistId
                    })
                    .OrderByDescending(a => a.StartTime)
                    .ToListAsync();

                return View(appointments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while fetching appointments.";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, string status)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);

                if (appointment == null)
                {
                    return Json(new { success = false, message = "Appointment not found." });
                }

                // Verify the appointment belongs to the current Therapist
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (appointment.TherapistId != userId)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                appointment.Status = status;
                appointment.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the status." });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var Therapist = await _userManager.FindByIdAsync(userId);

                if (Therapist == null)
                {
                    TempData["Error"] = "Therapist not found.";
                    return RedirectToAction("ManageSessions");
                }

                var appointment = await _context.Appointments
                    .Include(a => a.Student)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id && a.TherapistId == userId);

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found or unauthorized access.";
                    return RedirectToAction("ManageSessions");
                }

                var viewModel = new AppointmentApprovalViewModel
                {
                    AppointmentId = appointment.AppointmentId,
                    StudentName = $"{appointment.Student.FirstName} {appointment.Student.LastName}",
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    Status = appointment.Status,
                    TherapistNotes = ""  // New notes to be added during approval
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing the appointment.";
                return RedirectToAction("ManageSessions");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> ApproveAppointment(AppointmentApprovalViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId && a.TherapistId == userId);

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found or unauthorized access.";
                    return RedirectToAction("ManageSessions");
                }

                // Update appointment
                appointment.Status = "Approved";
                appointment.LastModified = DateTime.Now;
                appointment.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment approved successfully.";
                return RedirectToAction("ManageSessions");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while approving the appointment.";
                return View(model);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var appointment = await _context.Appointments
                    .Include(a => a.Student)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id && a.TherapistId == userId);

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found or unauthorized access.";
                    return RedirectToAction("ManageSessions");
                }

                var viewModel = new AppointmentCancelViewModel
                {
                    AppointmentId = appointment.AppointmentId,
                    StudentName = $"{appointment.Student.FirstName} {appointment.Student.LastName}",
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    CancellationReason = ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing the request.";
                return RedirectToAction("ManageSessions");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> CancelAppointment(AppointmentCancelViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId && a.TherapistId == userId);

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found or unauthorized access.";
                    return RedirectToAction("ManageSessions");
                }

                appointment.Status = "Cancelled";
                appointment.LastModified = DateTime.Now;
                appointment.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment cancelled successfully.";
                return RedirectToAction("ManageSessions");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while cancelling the appointment.";
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> EditAppointment(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var appointment = await _context.Appointments
                    .Include(a => a.Student)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id && a.TherapistId == userId);

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found or unauthorized access.";
                    return RedirectToAction("ManageSessions");
                }

                var viewModel = new AppointmentEditViewModel
                {
                    AppointmentId = appointment.AppointmentId,
                    StudentName = $"{appointment.Student.FirstName} {appointment.Student.LastName}",
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing the request.";
                return RedirectToAction("ManageSessions");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> EditAppointment(AppointmentEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId && a.TherapistId == userId);

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found or unauthorized access.";
                    return RedirectToAction("ManageSessions");
                }

                // Validate that the new time doesn't conflict with existing appointments
                var hasConflict = await _context.Appointments
                    .AnyAsync(a => a.TherapistId == userId &&
                                  a.AppointmentId != model.AppointmentId &&
                                  a.Status != "Cancelled" &&
                                  ((model.StartTime >= a.StartTime && model.StartTime < a.EndTime) ||
                                   (model.EndTime > a.StartTime && model.EndTime <= a.EndTime)));

                if (hasConflict)
                {
                    ModelState.AddModelError("", "The selected time conflicts with another appointment.");
                    return View(model);
                }

                appointment.StartTime = model.StartTime;
                appointment.EndTime = model.EndTime;
                appointment.LastModified = DateTime.Now;
                appointment.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment updated successfully.";
                return RedirectToAction("ManageSessions");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the appointment.";
                return View(model);
            }
        }
        public IActionResult PatientList()
        {
            return View();
        }
    }
}
