using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.Services;
using MindCare.ViewModel;
using System.Security.Claims;

namespace MindCare.Controllers
{
    public class TherapistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        public TherapistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _notificationService = notificationService;
        }
        public IActionResult Index()
        {
            return View("Index");
        }

        public async Task<IActionResult> PatientList()
        {
            try
            {
                // Get the logged-in Therapist's ID
                var TherapistId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get all unique patients who have appointments with this Therapist
                // Explicitly join with AspNetUsers table
                var patients = await _context.Appointments
                    .Where(a => a.TherapistId == TherapistId)
                    .Join(
                        _context.Users, // Join with AspNetUsers table
                        appointment => appointment.StudentId,
                        user => user.Id,
                        (appointment, user) => new { Appointment = appointment, User = user }
                    )
                    .GroupBy(x => x.User.Id)
                    .Select(g => new PatientViewModel
                    {
                        StudentId = g.Key,
                        Name = $"{g.First().User.FirstName} {g.First().User.LastName}",
                        Email = g.First().User.Email,
                        LastAppointment = g.Max(x => x.Appointment.StartTime),
                        TotalAppointments = g.Count()
                    })
                    .OrderByDescending(p => p.LastAppointment)
                    .ToListAsync();

                return View(patients);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while fetching the patient list.";
                // Log the exception details
                Console.WriteLine($"Error in PatientList: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return View(new List<PatientViewModel>());
            }
        }
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> ViewPatientHistory(string id)
        {
            try
            {
                // Get the logged-in Therapist's ID
                var TherapistId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get patient details from AspNetUsers
                var patient = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction("PatientList");
                }

                // Get all appointments for this patient with this Therapist
                var appointments = await _context.Appointments
                    .Where(a => a.StudentId == id && a.TherapistId == TherapistId)
                    .OrderByDescending(a => a.StartTime)
                    .Select(a => new AppointmentHistoryViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        CreatedAt = a.CreatedAt,
                        CancellationTime = a.CancellationTime
                    })
                    .ToListAsync();

                var viewModel = new PatientHistoryViewModel
                {
                    StudentId = patient.Id,
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    Email = patient.Email,
                    Appointments = appointments
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while fetching patient history.";
                return RedirectToAction("PatientList");
            }
        }

        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> ManageSessions()
        {
            try
            {
                // Get the logged-in Therapist's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Perform a more explicit join and mapping
                var appointments = await (from appointment in _context.Appointments
                                          join user in _context.Users on appointment.StudentId equals user.Id
                                          where appointment.TherapistId == userId && appointment.Status == "Pending"
                                          select new AppointmentViewModel
                                          {
                                              AppointmentId = appointment.AppointmentId,
                                              Date = appointment.Date,
                                              Time = appointment.Time,
                                              StartTime = appointment.StartTime,
                                              EndTime = appointment.EndTime,
                                              Status = appointment.Status,
                                              StudentId = user.Id,
                                              StudentName = user.FirstName + " " + user.LastName,
                                              StudentEmail = user.Email
                                          })
                                          .OrderByDescending(a => a.StartTime)
                                          .ToListAsync();

                return View(appointments);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in ManageSessions: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = "An error occurred while fetching appointments.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, string status)
        {
            try
            {
                // Get the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                // Find the appointment
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    return Json(new { success = false, message = $"Appointment with ID {appointmentId} not found." });
                }

                // Verify the appointment belongs to the current Therapist
                if (appointment.TherapistId != userId)
                {
                    return Json(new { success = false, message = "Unauthorized access: Appointment does not belong to current Therapist." });
                }

                // Update appointment status
                appointment.Status = status;
                appointment.LastModified = DateTime.UtcNow;
                appointment.UpdatedBy = userId;

                // Save changes
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Appointment status updated successfully.",
                    appointmentId = appointmentId,
                    newStatus = status
                });
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error in UpdateAppointmentStatus: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                // Get the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the user exists
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "User authentication failed."
                    });
                }

                // Find the appointment with detailed logging
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

                // Log detailed information for debugging
                if (appointment == null)
                {
                    Console.WriteLine($"Appointment not found. ID: {id}");
                    return Json(new
                    {
                        success = false,
                        message = $"Appointment with ID {id} not found."
                    });
                }

                // Additional authorization check
                if (appointment.TherapistId != userId)
                {
                    Console.WriteLine($"Unauthorized deletion attempt. " +
                        $"Appointment TherapistId: {appointment.TherapistId}, " +
                        $"Current User ID: {userId}");
                    return Json(new
                    {
                        success = false,
                        message = "You are not authorized to delete this appointment."
                    });
                }

                // Remove the appointment
                _context.Appointments.Remove(appointment);
                int result = await _context.SaveChangesAsync();

                // Log successful deletion
                Console.WriteLine($"Appointment {id} deleted successfully. Rows affected: {result}");

                return Json(new
                {
                    success = true,
                    message = "Appointment deleted successfully.",
                    appointmentId = id
                });
            }
            catch (Exception ex)
            {
                // Comprehensive error logging
                Console.WriteLine($"Exception in DeleteAppointment: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Return more detailed error information
                return Json(new
                {
                    success = false,
                    message = $"Deletion failed: {ex.Message}"
                });
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
        public async Task<IActionResult> ApproveAppointment(int appointmentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var appointment = await _context.Appointments
                    .Include(a => a.StudentId)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.TherapistId == userId);
                if (appointment != null)
                {
                    appointment.Status = "Approved";
                    await _context.SaveChangesAsync();

                    // Send notification
                    await _notificationService.SendAppointmentNotification(
                        appointment.StudentId,
                        appointment.StartTime,
                        "APPROVED"
                    );
                }

                if (appointment == null)
                {
                    return Json(new { success = false, message = "Appointment not found or unauthorized access." });
                }

                if (appointment.Status != "Pending")
                {
                    return Json(new { success = false, message = "Only pending appointments can be approved." });
                }

                // Check for time conflicts
                var hasConflict = await _context.Appointments
                    .AnyAsync(a => a.TherapistId == userId &&
                                  a.AppointmentId != appointmentId &&
                                  a.Status == "Approved" &&
                                  ((appointment.StartTime >= a.StartTime && appointment.StartTime < a.EndTime) ||
                                   (appointment.EndTime > a.StartTime && appointment.EndTime <= a.EndTime)));

                if (hasConflict)
                {
                    return Json(new { success = false, message = "Time slot conflicts with another approved appointment." });
                }

                // Update appointment status
                appointment.Status = "Approved";
                appointment.LastModified = DateTime.UtcNow;
                appointment.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                // Create notification for student
                var notification = new Notification
                {
                    UserId = appointment.StudentId,
                    Message = $"Your appointment scheduled for {appointment.StartTime:g} has been approved.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Appointment approved successfully.",
                    appointmentId = appointment.AppointmentId,
                    studentName = $"{appointment.Student.FirstName} {appointment.Student.LastName}",
                    startTime = appointment.StartTime.ToString("g"),
                    endTime = appointment.EndTime.ToString("g")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while approving the appointment." });
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
        [Authorize(Roles = "Therapist")]
        public async Task<IActionResult> ApprovedAppointments()
        {
            try
            {
                // Get the logged-in Therapist's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Perform an explicit join to include student details
                var appointments = await (from appointment in _context.Appointments
                                          join user in _context.Users on appointment.StudentId equals user.Id
                                          where appointment.TherapistId == userId && appointment.Status == "Approved"
                                          select new AppointmentViewModel
                                          {
                                              AppointmentId = appointment.AppointmentId,
                                              Date = appointment.Date,
                                              Time = appointment.Time,
                                              StartTime = appointment.StartTime,
                                              EndTime = appointment.EndTime,
                                              Status = appointment.Status,
                                              StudentId = user.Id,
                                              StudentName = user.FirstName + " " + user.LastName,
                                              StudentEmail = user.Email
                                          })
                                          .OrderByDescending(a => a.StartTime)
                                          .ToListAsync();

                return View(appointments);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in ApprovedAppointments: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = "An error occurred while fetching appointments.";
                return RedirectToAction("Index");
            }
        }

    }
}
