using Microsoft.AspNetCore.Mvc;
using MindCare.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MindCare.Data;

public class UserActivityController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserActivityController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> UserActivityReport()
    {
        // Retrieve user activities ordered by latest timestamp
        var activities = await _context.UserActivities
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return View(activities);
    }
}
