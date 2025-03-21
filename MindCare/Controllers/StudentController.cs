using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindCare.Models;

namespace MindCare.Controllers
{
    public class StudentController : Controller

    {
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task <IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["FirstName"] = user?.FirstName;
            return View();
        }
    }
}
