using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using MindCare.ViewModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace MindCare.Controllers
{
    
    public class TherapyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TherapyController> _logger;

        public TherapyController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TherapyController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Therapy/TherapySessions
        public async Task<IActionResult> TherapySessions()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<Appointment> appointmentsQuery = _context.Appointments
                .Include(a => a.Student)
                .Include(a => a.Therapist)
                .Include(a => a.Psychiatrist);

            if (userRoles.Contains("Student"))
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.StudentId == currentUser.Id);
            }
            else if (userRoles.Contains("Therapist"))
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.TherapistId == currentUser.Id);
            }
            else if (userRoles.Contains("Psychiatrist"))
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.PsychiatristId == currentUser.Id);
            }

            var sessions = await appointmentsQuery
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            return View(sessions);
        }

        
        public async Task<IActionResult> BookSession()
        {
            var therapists = await _userManager.GetUsersInRoleAsync("Therapist");

            if (!therapists.Any())
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "No therapists are available for booking at this time."
                });
            }

            var therapistList = therapists.Select(t => new TherapistViewModel
            {
                Id = t.Id,
                Name = $"{t.FirstName} {t.LastName}",
                Specialization = t.Specialization,
                Rating = t.Rating
            }).ToList();

            ViewData["Therapists"] = new SelectList(therapistList, "Id", "Name");

            var appointment = new Appointment
            {
                Date = DateTime.Today,
                Status = "Pending"
            };

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> BookSession([Bind("Date,Time,TherapistId")] Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return await ReloadBookingView(appointment);
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // Combine date and time for start time
                var localStartTime = appointment.Date.Add(appointment.Time);
                var localEndTime = localStartTime.AddHours(1);

                if (!IsValidAppointmentTime(localStartTime))
                {
                    ModelState.AddModelError("Date", "Invalid appointment time. Please select a valid future date and time during business hours (9 AM - 5 PM, weekdays only).");
                    return await ReloadBookingView(appointment);
                }

                // Create new appointment with all required fields
                var newAppointment = new Appointment
                {
                    Date = appointment.Date,
                    Time = appointment.Time,
                    StartTime = localStartTime.ToUniversalTime(),
                    EndTime = localEndTime.ToUniversalTime(),
                    Status = "Pending",
                    StudentId = currentUser.Id,
                    TherapistId = appointment.TherapistId
                };

                // Check for conflicts
                var conflictingAppointment = await _context.Appointments
                    .AnyAsync(a => a.TherapistId == appointment.TherapistId &&
                               a.Status != "Cancelled" &&
                               a.StartTime < newAppointment.EndTime &&
                               a.EndTime > newAppointment.StartTime);

                if (conflictingAppointment)
                {
                    ModelState.AddModelError("Time", "This time slot is no longer available. Please select another time.");
                    return await ReloadBookingView(appointment);
                }

                // Check for pending appointments
                var hasPendingAppointment = await _context.Appointments
                    .AnyAsync(a => a.StudentId == currentUser.Id &&
                                 a.Status == "Pending" &&
                                 a.StartTime > DateTime.UtcNow);

                if (hasPendingAppointment)
                {
                    ModelState.AddModelError("", "You already have a pending appointment. Please wait for confirmation or cancel it before booking a new one.");
                    return await ReloadBookingView(appointment);
                }

                _context.Appointments.Add(newAppointment);
                await _context.SaveChangesAsync();

                await SendAppointmentNotification(newAppointment);

                TempData["SuccessMessage"] = "Appointment booked successfully! Please wait for therapist confirmation.";
                return RedirectToAction(nameof(SessionDetails), new { id = newAppointment.AppointmentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while booking appointment: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while booking the appointment. Please try again.");
                return await ReloadBookingView(appointment);
            }
        }
        public async Task<IActionResult> SessionDetails(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Student)
                .Include(a => a.Therapist)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }


        private bool IsValidAppointmentTime(DateTime appointmentTime)
        {
            if (appointmentTime <= DateTime.Now)
                return false;

            if (appointmentTime.DayOfWeek == DayOfWeek.Saturday ||
                appointmentTime.DayOfWeek == DayOfWeek.Sunday)
                return false;

            var appointmentTimeOfDay = appointmentTime.TimeOfDay;
            var workingHourStart = new TimeSpan(9, 0, 0);
            var workingHourEnd = new TimeSpan(17, 0, 0);

            return appointmentTimeOfDay >= workingHourStart &&
                   appointmentTimeOfDay < workingHourEnd;
        }

        private async Task<IActionResult> ReloadBookingView(Appointment appointment)
        {
            var therapists = await _userManager.GetUsersInRoleAsync("Therapist");
            ViewData["Therapists"] = new SelectList(
                therapists.Select(t => new TherapistViewModel
                {
                    Id = t.Id,
                    Name = $"{t.FirstName} {t.LastName}",
                    Specialization = t.Specialization,
                    Rating = t.Rating
                }),
                "Id",
                "Name",
                appointment.TherapistId
            );
            return View(appointment);
        }

        private async Task SendAppointmentNotification(Appointment appointment)
        {
            try
            {
                var therapist = await _userManager.FindByIdAsync(appointment.TherapistId);
                var student = await _userManager.FindByIdAsync(appointment.StudentId);

                _logger.LogInformation(
                    "New appointment booked: Therapist: {TherapistName}, Student: {StudentName}, Time: {AppointmentTime}",
                    $"{therapist?.FirstName} {therapist?.LastName}",
                    $"{student?.FirstName} {student?.LastName}",
                    appointment.StartTime
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment notification");
            }
        }
    }
}
