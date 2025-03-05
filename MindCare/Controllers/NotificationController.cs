using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindCare.Models;
using MindCare.Services;

namespace MindCare.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = _userManager.GetUserId(User);
            var notifications = await _notificationService.GetUserNotifications(userId);
            return Json(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkAsRead(notificationId);
            return Ok();
        }

        // New method to mark all notifications as read
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);
            await _notificationService.MarkAllAsRead(userId);
            return Ok();
        }
    }
}