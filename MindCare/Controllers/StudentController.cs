using Microsoft.AspNetCore.Mvc;

namespace MindCare.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
