using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MindCare.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("~/Views/Error/404.cshtml");
                case 500:
                    return View("~/Views/Error/500.cshtml");
                case 401:
                    return View("~/Views/Error/401.cshtml");
                case 403:
                    return View("~/Views/Error/403.cshtml");
                default:
                    return View("~/Views/Error/500.cshtml");
            }
        }

        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Error/500.cshtml");
        }
    }
}