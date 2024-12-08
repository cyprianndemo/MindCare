using MindCare.Models;

namespace MindCare.Services
{
    public interface INotificationService
    {
        Task CreateNotification(string userId, string message, string type);
        Task<List<Notification>> GetUserNotifications(string userId);
        Task MarkAsRead(int notificationId);
        Task SendEmail(string email, string subject, string message);
    }
}
