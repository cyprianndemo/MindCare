using MindCare.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using Microsoft.AspNetCore.Identity;

public class UserActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserActivityService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogActivity(string username, string action, string description, int? relatedEntityId = null, string relatedEntityType = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user == null) return;

        // Get current IP address
        var ipAddress = GetUserIpAddress();

        // Get user agent
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        var activity = new UserActivity
        {
            UserId = user.Id,
            User = user,
            Action = action,
            Description = description,
            Timestamp = DateTime.UtcNow,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            RelatedEntityId = relatedEntityId?.ToString(),
            RelatedEntityType = relatedEntityType
        };


        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync();

        // If using SignalR for real-time updates, trigger notification here
    }

    private string GetUserIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ipAddress = forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? ipAddress;
        }

        return ipAddress ?? "Unknown";
    }
}