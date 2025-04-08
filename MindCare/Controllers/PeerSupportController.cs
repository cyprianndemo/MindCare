using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Hubs;
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
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<PeerSupportController> _logger;

        public PeerSupportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<MessageHub> hubContext,
                ILogger<PeerSupportController> logger)

        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
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
        public async Task<IActionResult> SendMessage([FromForm] string Content, [FromForm] int? ConversationId, [FromForm] string RecipientId)
        {
            try
            {
                // Log the received parameters
                Console.WriteLine($"DEBUG: Received - Content: {Content}, ConversationId: {ConversationId}, RecipientId: {RecipientId}");

                // Basic validation
                if (string.IsNullOrWhiteSpace(Content))
                {
                    return Json(new { success = false, error = "Message cannot be empty." });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                {
                    return Json(new { success = false, error = "Current user not found." });
                }

                var recipient = await _userManager.FindByIdAsync(RecipientId);
                if (recipient == null)
                {
                    return Json(new { success = false, error = "Recipient not found." });
                }

                // Find or create conversation - make sure this method properly creates or finds a conversation
                var conversation = await FindOrCreateConversation(userId, RecipientId);
                if (conversation == null || conversation.ConversationId <= 0)
                {
                    return Json(new { success = false, error = "Could not create or find conversation." });
                }

                Console.WriteLine($"DEBUG: Using conversation ID: {conversation.ConversationId}");

                // Create and save the message
                var message = new DirectMessage
                {
                    ConversationId = conversation.ConversationId,
                    SenderId = userId,
                    RecipientId = RecipientId,
                    Content = Content.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                // Add and save immediately to check for specific errors
                try
                {
                    _context.DirectMessages.Add(message);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG: Message saved with ID: {message.MessageId}");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"DEBUG: Database error saving message - {dbEx.Message}");
                    Console.WriteLine($"DEBUG: Inner exception - {dbEx.InnerException?.Message}");
                    return Json(new { success = false, error = $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}" });
                }

                // Create notification
                try
                {
                    var notification = new Notification
                    {
                        UserId = RecipientId,
                        Message = $"New message from {currentUser.FirstName} {currentUser.LastName}",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        Type = "Message",
                        Link = $"/PeerSupport/DirectMessages?peerId={userId}"
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("DEBUG: Notification created successfully");
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"DEBUG: Error creating notification - {notifEx.Message}");
                    // Continue even if notification fails - the message was already saved
                }

                // Send real-time notification via SignalR
                try
                {
                    await _hubContext.Clients.Group(conversation.ConversationId.ToString())
                        .SendAsync("ReceiveMessage",
                            message.MessageId.ToString(),
                            userId,
                            $"{currentUser.FirstName} {currentUser.LastName}",
                            currentUser.CurrentMoodIcon ?? "😊",
                            message.Content,
                            message.CreatedAt);
                    Console.WriteLine("DEBUG: SignalR message sent successfully");
                }
                catch (Exception signalREx)
                {
                    Console.WriteLine($"DEBUG: SignalR error - {signalREx.Message}");
                    // Continue execution even if SignalR fails - we'll rely on page refresh as fallback
                }

                return Json(new
                {
                    success = true,
                    messageId = message.MessageId,
                    content = message.Content,
                    createdAt = message.CreatedAt,
                    senderName = $"{currentUser.FirstName} {currentUser.LastName}",
                    senderMoodIcon = currentUser.CurrentMoodIcon ?? "😊"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in SendMessage - {ex.Message}");
                Console.WriteLine($"DEBUG: Stack trace - {ex.StackTrace}");
                return Json(new { success = false, error = $"Error sending message: {ex.Message}" });
            }
        }
        // Helper method to find or create a conversation
        private async Task<Conversation> FindOrCreateConversation(string userId, string peerId)
        {
            // Look for an existing conversation between these users
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == userId && c.User2Id == peerId) ||
                    (c.User1Id == peerId && c.User2Id == userId));

            // If no conversation exists, create a new one
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = userId,
                    User2Id = peerId,
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG: Created new conversation with ID: {conversation.ConversationId}");
            }
            else
            {
                // Update the LastActivityAt timestamp
                conversation.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG: Using existing conversation with ID: {conversation.ConversationId}");
            }

            return conversation;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            try
            {
                // Log the incoming messageId for debugging
                _logger.LogInformation($"Attempting to delete message with ID: {messageId}");

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("Delete message failed: User not authenticated");
                    return Json(new { success = false, error = "User not authenticated" });
                }

                if (string.IsNullOrEmpty(messageId))
                {
                    _logger.LogWarning("Delete message failed: Empty messageId");
                    return Json(new { success = false, error = "Message ID is required" });
                }

                // Try parsing the messageId to an int
                if (!int.TryParse(messageId, out int id))
                {
                    _logger.LogWarning($"Delete message failed: Invalid messageId format: {messageId}");
                    return Json(new { success = false, error = "Invalid Message ID format" });
                }

                // Now query using the parsed int
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (message == null)
                {
                    _logger.LogWarning($"Delete message failed: Message not found with ID: {id}");
                    return Json(new { success = false, error = "Message not found" });
                }

                // Verify the current user is the sender of the message
                if (message.SenderId != currentUserId)
                {
                    _logger.LogWarning($"Delete message failed: User {currentUserId} attempted to delete message {id} owned by {message.SenderId}");
                    return Json(new { success = false, error = "You can only delete your own messages" });
                }

                // Get conversation ID before deleting for SignalR notification
                var conversationId = message.ConversationId;

                // Delete the message
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Message {id} successfully deleted");

                // Notify other users in the conversation through SignalR
                await _hubContext.Clients.Group(conversationId).SendAsync("MessageDeleted", messageId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting message: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
 }