using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Hubs;
using MindCare.Models;
using System.Net.Mail;
using System.Net;

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

            // Send real-time notification
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Message,
                notification.CreatedAt,
                notification.Type
            });
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
    }
}
