using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Hubs;
using MindCare.Models;
using System.Net;
using System.Net.Mail;

namespace MindCare.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration _configuration;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> hubContext,
            IConfiguration configuration)
        {
            _context = context;
            _hubContext = hubContext;
            _configuration = configuration;
        }

        public async Task SendAppointmentNotification(string userId, DateTime appointmentTime, string notificationType)
        {
            (string title, string message) notification = notificationType switch
            {
                "BOOKED" => NotificationTemplates.GetAppointmentBookedTemplate(appointmentTime),
                "APPROVED" => NotificationTemplates.GetAppointmentApprovedTemplate(appointmentTime),
                "CANCELLED" => NotificationTemplates.GetAppointmentCancelledTemplate(appointmentTime),
                "REMINDER" => NotificationTemplates.GetAppointmentReminderTemplate(appointmentTime),
                _ => throw new ArgumentException("Invalid notification type", nameof(notificationType))
            };

            await CreateNotification(userId, notification.message, notificationType);
            await SendEmail(await GetUserEmail(userId), notification.title, notification.message);
        }

        public async Task SendAppointmentRescheduledNotification(string userId, DateTime oldTime, DateTime newTime)
        {
            var (title, message) = NotificationTemplates.GetAppointmentRescheduledTemplate(oldTime, newTime);
            await CreateNotification(userId, message, "RESCHEDULED");
            await SendEmail(await GetUserEmail(userId), title, message);
        }

        private async Task<string> GetUserEmail(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Email ?? throw new ArgumentException("User not found", nameof(userId));
        }

        public async Task CreateNotification(string userId, string message, string type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = type
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Message,
                notification.CreatedAt,
                notification.Type
            });
        }

        public async Task SendEmail(string email, string subject, string message)
        {
            using (var client = new SmtpClient())
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:From"]),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                client.Host = _configuration["Email:SmtpServer"];
                client.Port = int.Parse(_configuration["Email:Port"]);
                client.Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );
                client.EnableSsl = true;

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task<List<Notification>> GetUserNotifications(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
        public async Task MarkAllAsRead(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
