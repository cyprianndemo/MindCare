using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Hubs;
using MindCare.Models;

namespace MindCare.Controllers
{
    public class SupportGroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public SupportGroupController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var groups = await _context.SupportGroups
                .Include(g => g.Moderator)
                .ToListAsync();

            // Create a default group if none exist
            if (!groups.Any())
            {
                var defaultGroup = new SupportGroup
                {
                    Name = "General Support Group",
                    Description = "A safe space for general mental health discussion",
                    ModeratorId = currentUser.Id,  // Assign current user as moderator
                    Moderator = currentUser
                };

                _context.SupportGroups.Add(defaultGroup);
                await _context.SaveChangesAsync();
                groups = await _context.SupportGroups.ToListAsync();
            }

            return View(groups);
        }

        [Authorize]
        public async Task<IActionResult> Chat(int? groupId)
        {
            if (groupId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var group = await _context.SupportGroups
                .Include(g => g.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(g => g.Moderator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                group = new SupportGroup
                {
                    Name = "New Support Group",
                    Description = "A safe space for discussion",
                    ModeratorId = currentUser.Id,  // Assign current user as moderator
                    Moderator = currentUser,
                    Messages = new List<ChatMessage>()
                };

                _context.SupportGroups.Add(group);
                await _context.SaveChangesAsync();
            }

            return View(group);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(string name, string description)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var group = new SupportGroup
            {
                Name = name,
                Description = description,
                ModeratorId = currentUser.Id,  
                Moderator = currentUser
            };

            _context.SupportGroups.Add(group);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Chat), new { groupId = group.Id });
        }

    [HttpPost]
        public async Task<IActionResult> SendMessage(int groupId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            var message = new ChatMessage
            {
                SenderId = user.Id,
                SenderName = user.UserName,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsModerated = false
            };

            var group = await _context.SupportGroups.FindAsync(groupId);
            if (group == null)
                return NotFound();

            group.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast message to all group members
            await _hubContext.Clients.Group(groupId.ToString())
                .SendAsync("ReceiveMessage", message);

            return Json(new { success = true });
        }
    }

}
