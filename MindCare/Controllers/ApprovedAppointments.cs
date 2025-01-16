using Microsoft.AspNetCore.Mvc;

namespace MindCare.Controllers
{
    public class ApprovedAppointments : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
