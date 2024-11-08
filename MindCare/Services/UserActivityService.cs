using MindCare.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;

public class UserActivityService
{
    private readonly ApplicationDbContext _context;

    public UserActivityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActivity(string userId, string action, string description = null)
    {
        var activity = new UserActivity
        {
            UserId = userId,
            Action = action,
            Description = description,
            Timestamp = DateTime.Now
        };

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync();
    }
}
