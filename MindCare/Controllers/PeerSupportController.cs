using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;
using MindCare.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MindCare.Controllers
{
    [Authorize]
    public class PeerSupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PeerSupportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> SupportGroups()
        {
            var supportGroups = await _context.SupportGroups
                .Include(g => g.Members)
                .Select(g => new SupportGroupViewModel
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    Description = g.Description,
                    MemberCount = g.Members.Count,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();

            return View(supportGroups);
        }

        // View a specific support group with discussions
        public async Task<IActionResult> GroupDetails(int groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the group exists
            var group = await _context.SupportGroups
                .Include(g => g.Members)
                .Include(g => g.Discussions)
                    .ThenInclude(d => d.User)
                .Include(g => g.Discussions)
                    .ThenInclude(d => d.Responses)
                        .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                TempData["Error"] = "Support group not found.";
                return RedirectToAction(nameof(SupportGroups));
            }

            // Check if user is a member of the group
            var isMember = group.Members.Any(m => m.UserId == userId);

            var viewModel = new GroupDetailsViewModel
            {
                GroupId = group.GroupId,
                Name = group.Name,
                Description = group.Description,
                IsMember = isMember,
                MemberCount = group.Members.Count,
                Discussions = group.Discussions.Select(d => new DiscussionViewModel
                {
                    DiscussionId = d.DiscussionId,
                    Title = d.Title,
                    Content = d.Content,
                    CreatedAt = d.CreatedAt,
                    UserName = $"{d.User.FirstName} {d.User.LastName}",
                    ResponseCount = d.Responses.Count,
                    LatestResponseAt = d.Responses.Any() ? d.Responses.Max(r => r.CreatedAt) : d.CreatedAt
                }).OrderByDescending(d => d.LatestResponseAt).ToList()
            };

            return View(viewModel);
        }

        // Join a support group
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinGroup(int groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if already a member
            var existingMembership = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (existingMembership)
            {
                TempData["Info"] = "You are already a member of this group.";
                return RedirectToAction(nameof(GroupDetails), new { groupId });
            }

            // Add user to group
            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "You have successfully joined this support group.";
            return RedirectToAction(nameof(GroupDetails), new { groupId });
        }

        // Leave a support group
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (membership != null)
            {
                _context.GroupMembers.Remove(membership);
                await _context.SaveChangesAsync();
                TempData["Success"] = "You have left the support group.";
            }

            return RedirectToAction(nameof(SupportGroups));
        }

        // Create a new discussion in a group
        public async Task<IActionResult> CreateDiscussion(int groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (!isMember)
            {
                TempData["Error"] = "You must join the group before starting a discussion.";
                return RedirectToAction(nameof(GroupDetails), new { groupId });
            }

            var viewModel = new CreateDiscussionViewModel
            {
                GroupId = groupId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscussion(CreateDiscussionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == model.GroupId && m.UserId == userId);

            if (!isMember)
            {
                TempData["Error"] = "You must join the group before starting a discussion.";
                return RedirectToAction(nameof(GroupDetails), new { groupId = model.GroupId });
            }

            var discussion = new Discussion
            {
                GroupId = model.GroupId,
                UserId = userId,
                Title = model.Title,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Discussions.Add(discussion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your discussion has been posted.";
            return RedirectToAction(nameof(DiscussionDetails), new { discussionId = discussion.DiscussionId });
        }

        // View a discussion with responses
        public async Task<IActionResult> DiscussionDetails(int discussionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var discussion = await _context.Discussions
                .Include(d => d.User)
                .Include(d => d.Group)
                .Include(d => d.Responses)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(d => d.DiscussionId == discussionId);

            if (discussion == null)
            {
                TempData["Error"] = "Discussion not found.";
                return RedirectToAction(nameof(SupportGroups));
            }

            // Check if user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == discussion.GroupId && m.UserId == userId);

            if (!isMember)
            {
                TempData["Error"] = "You must join the group to view discussions.";
                return RedirectToAction(nameof(GroupDetails), new { groupId = discussion.GroupId });
            }

            var viewModel = new DiscussionDetailsViewModel
            {
                DiscussionId = discussion.DiscussionId,
                GroupId = discussion.GroupId,
                GroupName = discussion.Group.Name,
                Title = discussion.Title,
                Content = discussion.Content,
                CreatedAt = discussion.CreatedAt,
                UserName = $"{discussion.User.FirstName} {discussion.User.LastName}",
                UserMoodIcon = discussion.User.CurrentMoodIcon,
                Responses = discussion.Responses.Select(r => new ResponseViewModel
                {
                    ResponseId = r.ResponseId,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    UserMoodIcon = r.User.CurrentMoodIcon,
                    IsAuthor = r.UserId == userId
                }).OrderBy(r => r.CreatedAt).ToList(),
                NewResponse = new CreateResponseViewModel
                {
                    DiscussionId = discussionId
                }
            };

            return View(viewModel);
        }

        // Add a response to a discussion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResponse(CreateResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter a valid response.";
                return RedirectToAction(nameof(DiscussionDetails), new { discussionId = model.DiscussionId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the discussion and verify user is a member of the group
            var discussion = await _context.Discussions
                .Include(d => d.Group)
                .FirstOrDefaultAsync(d => d.DiscussionId == model.DiscussionId);

            if (discussion == null)
            {
                TempData["Error"] = "Discussion not found.";
                return RedirectToAction(nameof(SupportGroups));
            }

            var isMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == discussion.GroupId && m.UserId == userId);

            if (!isMember)
            {
                TempData["Error"] = "You must join the group to respond to discussions.";
                return RedirectToAction(nameof(GroupDetails), new { groupId = discussion.GroupId });
            }

            var response = new MindCare.Models.Response
            {
                DiscussionId = model.DiscussionId,
                UserId = userId,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            // Notify the discussion author if it's not their own response
            if (discussion.UserId != userId)
            {
                var notification = new Notification
                {
                    UserId = discussion.UserId,
                    Message = "Someone responded to your discussion in the support group.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Type = "Discussion",
                    Link = $"/PeerSupport/DiscussionDetails/{discussion.DiscussionId}"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Your response has been posted.";
            return RedirectToAction(nameof(DiscussionDetails), new { discussionId = model.DiscussionId });
        }

        // Find peers based on similar experiences or conditions
        // Find peers based on similar experiences or conditions
        public async Task<IActionResult> FindPeers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get all users with Student role
            var studentsWithRole = await _userManager.GetUsersInRoleAsync("Student");

            // Filter out the current user and transform to view model
            var peers = studentsWithRole
                .Where(user => user.Id != userId)
                .Select(user => new PeerViewModel
                {
                    UserId = user.Id,
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    CurrentMood = user.CurrentMood,
                    MoodIcon = user.CurrentMoodIcon ?? "😊", // Default emoji if null
                    Biography = string.Empty, // Not using MentalHealthProfiles
                    Conditions = string.Empty, // Not using MentalHealthProfiles
                    JourneyProgress = 0, // Not using MentalHealthProfiles
                    LastActive = user.LastLoginDate
                })
                .ToList();

            return View(peers);
        }

        // View and update current mood/state
        public async Task<IActionResult> UpdateMoodState()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var viewModel = new UpdateMoodViewModel
            {
                CurrentMood = user.CurrentMood,
                MoodIcon = user.CurrentMoodIcon
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMoodState(UpdateMoodViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            user.CurrentMood = model.CurrentMood;
            user.CurrentMoodIcon = model.MoodIcon;

            // Create a mood log entry
            var moodLog = new MoodLog
            {
                UserId = userId,
                MoodRating = model.MoodRating,
                MoodDescription = model.CurrentMood,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.MoodLogs.Add(moodLog);
            await _context.SaveChangesAsync();
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Your mood state has been updated.";
            return RedirectToAction(nameof(FindPeers));
        }

        // Direct message another peer
        public async Task<IActionResult> DirectMessages(string peerId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get all conversations for current user
            var conversations = await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Select(c => new
                {
                    Conversation = c,
                    OtherUserId = c.User1Id == userId ? c.User2Id : c.User1Id
                })
                .Join(
                    _context.Users,
                    combined => combined.OtherUserId,
                    user => user.Id,
                    (combined, user) => new ConversationViewModel
                    {
                        ConversationId = combined.Conversation.ConversationId,
                        OtherUserId = user.Id,
                        OtherUserName = $"{user.FirstName} {user.LastName}",
                        MoodIcon = user.CurrentMoodIcon,
                        LastMessagePreview = _context.DirectMessages
                            .Where(m => m.ConversationId == combined.Conversation.ConversationId)
                            .OrderByDescending(m => m.CreatedAt)
                            .Select(m => m.Content.Length > 30 ? m.Content.Substring(0, 27) + "..." : m.Content)
                            .FirstOrDefault() ?? "No messages yet",
                        LastMessageTime = _context.DirectMessages
                            .Where(m => m.ConversationId == combined.Conversation.ConversationId)
                            .OrderByDescending(m => m.CreatedAt)
                            .Select(m => m.CreatedAt)
                            .FirstOrDefault(),
                        UnreadCount = _context.DirectMessages
                            .Count(m => m.ConversationId == combined.Conversation.ConversationId &&
                                        m.RecipientId == userId &&
                                        !m.IsRead)
                    }
                )
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync();

            // If peerId is provided, get or create a conversation with that peer
            ConversationDetailsViewModel activeConversation = null;

            if (!string.IsNullOrEmpty(peerId))
            {
                // Check if peer exists
                var peer = await _userManager.FindByIdAsync(peerId);
                if (peer == null)
                {
                    TempData["Error"] = "User not found.";
                    return View(new MessagesViewModel { Conversations = conversations });
                }

                // Find existing conversation or create a new one
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c =>
                        (c.User1Id == userId && c.User2Id == peerId) ||
                        (c.User2Id == userId && c.User1Id == peerId));

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        User1Id = userId,
                        User2Id = peerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync();
                }

                // Get messages for this conversation
                var messages = await _context.DirectMessages
                    .Where(m => m.ConversationId == conversation.ConversationId)
                    .OrderBy(m => m.CreatedAt)
                    .Join(
                        _context.Users,
                        message => message.SenderId,
                        user => user.Id,
                        (message, user) => new MessageViewModel
                        {
                            MessageId = message.MessageId,
                            Content = message.Content,
                            CreatedAt = message.CreatedAt,
                            SenderName = $"{user.FirstName} {user.LastName}",
                            SenderMoodIcon = user.CurrentMoodIcon,
                            IsFromCurrentUser = message.SenderId == userId
                        }
                    )
                    .ToListAsync();

                // Mark unread messages as read
                var unreadMessages = await _context.DirectMessages
                    .Where(m => m.ConversationId == conversation.ConversationId &&
                               m.RecipientId == userId &&
                               !m.IsRead)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();

                activeConversation = new ConversationDetailsViewModel
                {
                    ConversationId = conversation.ConversationId,
                    OtherUserId = peerId,
                    OtherUserName = $"{peer.FirstName} {peer.LastName}",
                    MoodIcon = peer.CurrentMoodIcon,
                    Messages = messages,
                    NewMessage = new CreateMessageViewModel
                    {
                        ConversationId = conversation.ConversationId,
                        RecipientId = peerId
                    }
                };
            }

            var viewModel = new MessagesViewModel
            {
                Conversations = conversations,
                ActiveConversation = activeConversation
            };

            return View(viewModel);
        }

        // Send a direct message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(CreateMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter a valid message.";
                return RedirectToAction(nameof(DirectMessages), new { peerId = model.RecipientId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify the conversation exists and involves the current user
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == model.ConversationId &&
                                         (c.User1Id == userId || c.User2Id == userId));

            if (conversation == null)
            {
                TempData["Error"] = "Conversation not found.";
                return RedirectToAction(nameof(DirectMessages));
            }

            // Create and save the message
            var message = new DirectMessage
            {
                ConversationId = model.ConversationId,
                SenderId = userId,
                RecipientId = model.RecipientId,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.DirectMessages.Add(message);
            await _context.SaveChangesAsync();

            // Create notification for recipient
            var notification = new Notification
            {
                UserId = model.RecipientId,
                Message = "You have a new direct message.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = "Message",
                Link = $"/PeerSupport/DirectMessages?peerId={userId}"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DirectMessages), new { peerId = model.RecipientId });
        }
    }
}