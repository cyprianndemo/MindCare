using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MindCare.Controllers
{

    public class PsychiatristController : Controller
    {
        [Authorize(Roles = "Psychiatrist")]
        public IActionResult Dashboard()
        {
            return View("Dashboard"); 
        }
        public IActionResult ManageSessions()
        {
            // Logic to manage therapy sessions
            return View();
        }

        public IActionResult PatientList()
        {
            // Logic to get the list of patients assigned to the psychiatrist
            return View();
        }

        public IActionResult Prescriptions()
        {
            // Logic to handle prescriptions
            return View();
        }

        public IActionResult PatientFeedback()
        {
            // Logic to view feedback provided by patients
            return View();
        }
    }
}
