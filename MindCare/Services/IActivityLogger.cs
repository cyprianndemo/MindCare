using Microsoft.AspNetCore.SignalR;
using MindCare.Data;
using MindCare.Hubs;
using MindCare.Models;

namespace MindCare.Services
{
    public interface IActivityLogger
    {
        Task LogActivity(string userId, string action, string description, string relatedEntityId = null, string relatedEntityType = null);
    }

    public class ActivityLoggerV2 : IActivityLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLoggerV2(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task LogActivity(string userId, string action, string description, string relatedEntityId = null, string relatedEntityType = null)
        {
            throw new NotImplementedException();
        }

        public class ActivityLogger : IActivityLogger
        {
            private readonly ApplicationDbContext _context;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IHubContext<ActivityHub> _hubContext;

            public ActivityLogger(
                ApplicationDbContext context,
                IHttpContextAccessor httpContextAccessor,
                IHubContext<ActivityHub> hubContext)
            {
                _context = context;
                _httpContextAccessor = httpContextAccessor;
                _hubContext = hubContext;
            }

            public async Task LogActivity(string userId, string action, string description, string relatedEntityId = null, string relatedEntityType = null)
            {
                var httpContext = _httpContextAccessor.HttpContext;

                var activity = new UserActivity
                {
                    UserId = userId,
                    Action = action,
                    Description = description,
                    Timestamp = DateTime.UtcNow,
                    IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType
                };

                await _context.UserActivities.AddAsync(activity);
                await _context.SaveChangesAsync();

                // Get user details for real-time update
                var user = await _context.Users.FindAsync(userId);

                // Broadcast to all connected clients
                var activityViewModel = new
                {
                    id = activity.Id,
                    user = new
                    {
                        userName = user.UserName,
                        firstName = user.FirstName,
                        lastName = user.LastName
                    },
                    action = activity.Action,
                    description = activity.Description,
                    timestamp = activity.Timestamp,
                    ipAddress = activity.IPAddress
                };

                await _hubContext.Clients.All.SendAsync("ReceiveActivity", activityViewModel);
            }
        }
    }
}
