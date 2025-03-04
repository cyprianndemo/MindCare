// TherapyController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MindCare.Controllers
{
    [Authorize]
    public class TherapyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public TherapyController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> TherapySessions(string status = "All")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _context.Appointments
                .Include(a => a.Therapist)
                .Where(a => a.StudentId == userId &&
                            a.TherapistId != null &&
                            a.PsychiatristId == null);  // Only include therapy appointments

            // Filter by status if specified
            if (status == "Pending")
            {
                query = query.Where(a => a.Status == "Pending");
            }
            else if (status == "Approved")
            {
                query = query.Where(a => a.Status == "Approved");
            }
            // If status is "All" or not specified, no additional filtering needed

            var appointments = await query.OrderByDescending(a => a.StartTime).ToListAsync();
            ViewBag.CurrentStatus = status;

            return View(appointments);
        }

        public async Task<IActionResult> BookSession()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(userId);

            var appointment = new Appointment
            {
                StudentId = userId,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            var therapistUsers = await _userManager.GetUsersInRoleAsync("Therapist");
            ViewBag.Therapists = therapistUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

            var psychiatristUsers = await _userManager.GetUsersInRoleAsync("Psychiatrist");
            ViewBag.Psychiatrists = psychiatristUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

            ViewBag.UserEmail = currentUser.Email;
            ViewBag.StudentId = userId;

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookSession(Appointment appointment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.StudentId = userId;

            // Ensure all times are in UTC
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.LastModified = DateTime.UtcNow;
            appointment.Status = "Pending";

            // Convert the appointment times to UTC
            appointment.SetAppointmentTimes(appointment.Date, appointment.Time);

            // Ensure all DateTime fields are explicitly UTC
            appointment.EnsureUtcTimes();

            // Check if user already has an appointment at the same time or within an hour
            bool hasConflict = await CheckForTimeConflict(userId, appointment.StartTime, appointment.EndTime);
            if (hasConflict)
            {
                ModelState.AddModelError("", "You already have an appointment scheduled at this time or within an hour. Please choose a different time.");

                // Repopulate dropdown lists
                var therapistUsers = await _userManager.GetUsersInRoleAsync("Therapist");
                ViewBag.Therapists = therapistUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                var psychiatristUsers = await _userManager.GetUsersInRoleAsync("Psychiatrist");
                ViewBag.Psychiatrists = psychiatristUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                var currentUser = await _userManager.FindByIdAsync(userId);
                ViewBag.UserEmail = currentUser.Email;
                ViewBag.StudentId = userId;

                return View(appointment);
            }

            // Check if the therapist is already booked at this time
            bool therapistBooked = false;
            if (!string.IsNullOrEmpty(appointment.TherapistId))
            {
                therapistBooked = await IsTherapistBooked(appointment.TherapistId, appointment.StartTime, appointment.EndTime);
                if (therapistBooked)
                {
                    ModelState.AddModelError("TherapistId", "This therapist is already booked at the selected time. Please choose a different therapist or time.");

                    // Repopulate dropdown lists
                    var therapistUsers = await _userManager.GetUsersInRoleAsync("Therapist");
                    ViewBag.Therapists = therapistUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                    var psychiatristUsers = await _userManager.GetUsersInRoleAsync("Psychiatrist");
                    ViewBag.Psychiatrists = psychiatristUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                    var currentUser = await _userManager.FindByIdAsync(userId);
                    ViewBag.UserEmail = currentUser.Email;
                    ViewBag.StudentId = userId;

                    return View(appointment);
                }
            }

            // Save the appointment
            _context.Add(appointment);
            await _context.SaveChangesAsync();

            await SendAppointmentConfirmationEmail(appointment);

            // Redirect to payment page with the newly created appointment ID
            return RedirectToAction("Pay", "Checkout", new { appointmentId = appointment.AppointmentId });
        }

        // Check if the user already has an appointment at the same time or within an hour
        private async Task<bool> CheckForTimeConflict(string userId, DateTime startTime, DateTime endTime)
        {
            // Look for active appointments (not cancelled) within one hour before or after
            var oneHourBefore = startTime.AddHours(-1);
            var oneHourAfter = endTime.AddHours(1);

            var existingAppointments = await _context.Appointments
                .Where(a => a.StudentId == userId &&
                        a.Status != "Cancelled" &&
                        ((a.StartTime <= oneHourAfter && a.EndTime >= oneHourBefore) ||
                         (a.StartTime >= oneHourBefore && a.StartTime <= oneHourAfter) ||
                         (a.EndTime >= oneHourBefore && a.EndTime <= oneHourAfter)))
                .AnyAsync();

            return existingAppointments;
        }

        // Check if the therapist is already booked at this time
        private async Task<bool> IsTherapistBooked(string therapistId, DateTime startTime, DateTime endTime)
        {
            return await _context.Appointments
                .Where(a => a.TherapistId == therapistId &&
                        a.Status != "Cancelled" &&
                        a.StartTime < endTime &&
                        a.EndTime > startTime)
                .AnyAsync();
        }

        // GET: Edit Appointment
        public async Task<IActionResult> Edit(int id)
        {
            // Retrieve the appointment to be edited
            var appointment = await _context.Appointments
                .Include(a => a.Therapist)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Make sure the current user is the one who booked the appointment or is an admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (appointment.StudentId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Users can only edit their own appointments
            }

            // Load therapists and psychiatrists for dropdowns
            var therapistUsers = await _userManager.GetUsersInRoleAsync("Therapist");
            ViewBag.Therapists = therapistUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

            var psychiatristUsers = await _userManager.GetUsersInRoleAsync("Psychiatrist");
            ViewBag.Psychiatrists = psychiatristUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

            return View(appointment);
        }

        // POST: Edit Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound();
            }

            // Check if the current user owns the appointment or is an admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingAppointment = await _context.Appointments.FindAsync(id);

            if (existingAppointment == null)
            {
                return NotFound();
            }

            if (existingAppointment.StudentId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Users can only edit their own appointments
            }

            // Convert appointment times to UTC for validation
            var tempAppointment = new Appointment
            {
                AppointmentId = appointment.AppointmentId,
                Date = appointment.Date,
                Time = appointment.Time,
                TherapistId = appointment.TherapistId
            };
            tempAppointment.SetAppointmentTimes(appointment.Date, appointment.Time);
            tempAppointment.EnsureUtcTimes();

            // Check if the new time conflicts with other appointments (excluding this one)
            bool hasConflict = await _context.Appointments
                .Where(a => a.StudentId == userId &&
                            a.AppointmentId != id &&
                            a.Status != "Cancelled")
                .Where(a => (tempAppointment.StartTime <= a.EndTime.AddHours(1) &&
                            tempAppointment.EndTime >= a.StartTime.AddHours(-1)))
                .AnyAsync();

            if (hasConflict)
            {
                ModelState.AddModelError("", "The new appointment time conflicts with another appointment within an hour. Please choose a different time.");

                // Repopulate dropdown lists
                var therapistUsers = await _userManager.GetUsersInRoleAsync("Therapist");
                ViewBag.Therapists = therapistUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                var psychiatristUsers = await _userManager.GetUsersInRoleAsync("Psychiatrist");
                ViewBag.Psychiatrists = psychiatristUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                return View(appointment);
            }

            // Check if the therapist is already booked (excluding this appointment)
            if (!string.IsNullOrEmpty(appointment.TherapistId))
            {
                bool therapistBooked = await _context.Appointments
                    .Where(a => a.TherapistId == appointment.TherapistId &&
                                a.AppointmentId != id &&
                                a.Status != "Cancelled")
                    .Where(a => (tempAppointment.StartTime < a.EndTime &&
                                tempAppointment.EndTime > a.StartTime))
                    .AnyAsync();

                if (therapistBooked)
                {
                    ModelState.AddModelError("TherapistId", "This therapist is already booked at the selected time. Please choose a different therapist or time.");

                    // Repopulate dropdown lists
                    var therapistUsers = await _userManager.GetUsersInRoleAsync("Therapist");
                    ViewBag.Therapists = therapistUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                    var psychiatristUsers = await _userManager.GetUsersInRoleAsync("Psychiatrist");
                    ViewBag.Psychiatrists = psychiatristUsers.Select(u => new SelectListItem { Text = $"{u.FirstName} {u.LastName}", Value = u.Id }).ToList();

                    return View(appointment);
                }
            }

            // Update appointment details
            existingAppointment.TherapistId = appointment.TherapistId;
            existingAppointment.PsychiatristId = appointment.PsychiatristId;
            existingAppointment.Date = appointment.Date;
            existingAppointment.Time = appointment.Time;
            existingAppointment.Status = "Pending"; // Reset status as needed
            existingAppointment.LastModified = DateTime.UtcNow;

            // Convert appointment times to UTC
            existingAppointment.SetAppointmentTimes(appointment.Date, appointment.Time);
            existingAppointment.EnsureUtcTimes();

            // Save changes to the database
            _context.Update(existingAppointment);
            await _context.SaveChangesAsync();

            // Send confirmation email for the update
            await SendAppointmentUpdateEmail(existingAppointment);

            return RedirectToAction(nameof(TherapySessions));
        }
        // Add this method to your TherapyController class
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Make sure the current user is the one who booked the appointment or is an admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (appointment.StudentId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Users can only delete their own appointments
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            // Optionally send notification email about deletion
            // await SendAppointmentDeletionEmail(appointment);

            return RedirectToAction(nameof(TherapySessions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.CancellationTime = DateTime.UtcNow;
                appointment.CancelledById = userId;
                appointment.LastModified = DateTime.UtcNow;

                // Ensure all DateTime fields are UTC
                appointment.EnsureUtcTimes();

                await _context.SaveChangesAsync();
                await SendAppointmentCancellationEmail(appointment);
            }

            return RedirectToAction(nameof(TherapySessions));
        }
        public async Task<IActionResult> SessionDetails(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Therapist)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Generate a video call link (using Jitsi Meet as an example)
            // Customize the link generation logic if using other services
            string videoCallRoom = $"MindCare_{appointment.AppointmentId}_{appointment.TherapistId}";
            ViewBag.VideoCallLink = $"https://meet.jit.si/{videoCallRoom}";

            return View(appointment);
        }

        private async Task SendAppointmentConfirmationEmail(Appointment appointment)
        {
            var user = await _userManager.FindByIdAsync(appointment.StudentId);
            var therapist = await _userManager.FindByIdAsync(appointment.TherapistId);

            var emailSettings = _configuration.GetSection("EmailSettings");

            // Validate Email Settings
            var host = emailSettings["Host"];
            var portString = emailSettings["Port"];
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var fromAddress = emailSettings["FromAddress"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portString) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromAddress))
            {
                throw new InvalidOperationException("Email settings are incomplete. Please check your configuration.");
            }

            // Parse port if valid
            if (!int.TryParse(portString, out var port))
            {
                throw new InvalidOperationException($"Invalid port value: {portString}");
            }

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
            };

            var localStartTime = appointment.GetLocalStartTime();
            var localEndTime = appointment.GetLocalEndTime();

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = "Appointment Confirmation",
                Body = $@"Dear {user.FirstName},

                Your appointment has been scheduled successfully.

                Details:
                Date: {localStartTime:dd/MM/yyyy}
                Time: {localStartTime:HH:mm} - {localEndTime:HH:mm}
                Therapist: {therapist.FirstName} {therapist.LastName}

                Best regards,
                MindCare Team",
                IsBodyHtml = false
            };

            mailMessage.To.Add(user.Email);
            await smtpClient.SendMailAsync(mailMessage);
        }

        private async Task SendAppointmentUpdateEmail(Appointment appointment)
        {
            var user = await _userManager.FindByIdAsync(appointment.StudentId);
            var therapist = await _userManager.FindByIdAsync(appointment.TherapistId);

            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpClient = new SmtpClient(emailSettings["Host"])
            {
                Port = int.Parse(emailSettings["Port"]),
                Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["FromAddress"]),
                Subject = "Appointment Update",
                Body = $@"Dear {user.FirstName},

                Your appointment has been updated.

                New Details:
                Date: {appointment.StartTime.ToLocalTime():dd/MM/yyyy}
                Time: {appointment.StartTime.ToLocalTime():HH:mm} - {appointment.EndTime.ToLocalTime():HH:mm}
                Therapist: {therapist.FirstName} {therapist.LastName}

                Best regards,
                MindCare Team",
                IsBodyHtml = false
            };

            mailMessage.To.Add(user.Email);
            await smtpClient.SendMailAsync(mailMessage);
        }

        private async Task SendAppointmentCancellationEmail(Appointment appointment)
        {
            var user = await _userManager.FindByIdAsync(appointment.StudentId);
            var therapist = await _userManager.FindByIdAsync(appointment.TherapistId);

            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpClient = new SmtpClient(emailSettings["Host"])
            {
                Port = int.Parse(emailSettings["Port"]),
                Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["FromAddress"]),
                Subject = "Appointment Cancellation",
                Body = $@"Dear {user.FirstName},

                Your appointment has been cancelled.

                Cancelled Appointment Details:
                Date: {appointment.StartTime.ToLocalTime():dd/MM/yyyy}
                Time: {appointment.StartTime.ToLocalTime():HH:mm} - {appointment.EndTime.ToLocalTime():HH:mm}
                Therapist: {therapist.FirstName} {therapist.LastName}

                Best regards,
                MindCare Team",
                IsBodyHtml = false
            };

            mailMessage.To.Add(user.Email);
            await smtpClient.SendMailAsync(mailMessage);
        }

    }
}