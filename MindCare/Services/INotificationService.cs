using MindCare.Models;

namespace MindCare.Services
{
    public interface INotificationService
    {
        Task SendAppointmentNotification(string userId, DateTime appointmentTime, string notificationType);
        Task SendAppointmentRescheduledNotification(string userId, DateTime oldTime, DateTime newTime);
        Task CreateNotification(string userId, string message, string type);
        Task<List<Notification>> GetUserNotifications(string userId);
        Task MarkAsRead(int notificationId);
        Task SendEmail(string email, string subject, string message);
    }
}