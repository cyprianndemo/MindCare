using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Hubs;
using MindCare.Models;
using MindCare.ViewModel;

namespace MindCare.Controllers
{
        public class MessagingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _hubContext = hubContext;
        }

        [Authorize]
        public async Task<IActionResult> UsersList()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // First, get the users without roles
            var users = await _userManager.Users
                .Where(u => u.Id != currentUser.Id)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Roles = new List<string>() // Initialize empty roles list
                })
                .ToListAsync();

            // Then, fetch roles for each user separately
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                user.Roles = await _userManager.GetRolesAsync(appUser);
            }

            return View(users);
        }

        [Authorize]
        public async Task<IActionResult> Conversation(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var otherUser = await _userManager.FindByIdAsync(userId);

            if (otherUser == null)
                return NotFound();

            var messages = await _context.DirectMessages
                .Where(m =>
                    (m.SenderId == currentUser.Id && m.RecipientId == userId) ||
                    (m.SenderId == userId && m.RecipientId == currentUser.Id))
                .OrderBy(m => m.Timestamp)
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .ToListAsync();

            // Mark unread messages as read
            var unreadMessages = messages
                .Where(m => m.RecipientId == currentUser.Id && !m.IsRead);
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            var viewModel = new ConversationViewModel
            {
                CurrentUser = currentUser,
                OtherUser = otherUser,
                Messages = messages
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string RecipientId, string content)
        {
            var sender = await _userManager.GetUserAsync(User);
            var Recipient = await _userManager.FindByIdAsync(RecipientId);

            if (Recipient == null)
                return NotFound();

            var message = new DirectMessage
            {
                SenderId = sender.Id,
                RecipientId = RecipientId,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.DirectMessages.Add(message);
            await _context.SaveChangesAsync();

            // Notify the Recipient if they're online
            await _hubContext.Clients.User(RecipientId).SendAsync(
                "ReceiveMessage",
                new
                {
                    senderId = sender.Id,
                    senderName = sender.UserName,
                    content = content,
                    timestamp = message.Timestamp
                });

            return Json(new { success = true });
        }

        [Authorize]
        public async Task<IActionResult> UnreadMessages()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var unreadMessages = await _context.DirectMessages
                .Where(m => m.RecipientId == currentUser.Id && !m.IsRead)
                .Include(m => m.Sender)
                .GroupBy(m => m.SenderId)
                .Select(g => new UnreadMessagesViewModel
                {
                    SenderId = g.Key,
                    SenderName = g.First().Sender.UserName,
                    Count = g.Count()
                })
                .ToListAsync();

            return Json(unreadMessages);
        }
    }
}
