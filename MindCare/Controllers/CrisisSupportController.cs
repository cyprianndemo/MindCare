using Microsoft.AspNetCore.Mvc;
using MindCare.Crisis;

namespace MindCare.Controllers
{
    public class CrisisSupportController : Controller
    {
        // GET: CrisisSupport
        public ActionResult Index()
        {
            var supportResources = new List<SupportResourceModel>
            {
                new SupportResourceModel
                {
                    Name = "National Emergency Services",
                    PhoneNumber = "999 or 112",
                    Description = "Centralized emergency response for police, ambulance, and fire services",
                    Category = "Emergency Services"
                },
                new SupportResourceModel
                {
                    Name = "Befrienders Kenya (Mental Health Support)",
                    PhoneNumber = "+254 722 178 177",
                    Description = "Free confidential emotional support and suicide prevention helpline",
                    Website = "https://www.befrienderskenya.org/",
                    Category = "Mental Health"
                },
                new SupportResourceModel
                {
                    Name = "Gender Violence Recovery Centre",
                    PhoneNumber = "+254 20 2716007",
                    Description = "Support for survivors of gender-based violence",
                    Website = "https://www.fida.org.ke/",
                    Category = "Gender-Based Violence"
                },
                new SupportResourceModel
                {
                    Name = "Child Helpline Kenya",
                    PhoneNumber = "116",
                    Description = "Free, 24/7 helpline for children in need of support or protection",
                    Category = "Child Protection"
                },
                new SupportResourceModel
                {
                    Name = "Kenya Red Cross Helpline",
                    PhoneNumber = "1199",
                    Description = "Humanitarian support during emergencies and disasters",
                    Website = "https://www.kenyaredcross.org/",
                    Category = "Humanitarian Aid"
                }
            };

            return View(supportResources);
        }

        // Action to display detailed resource information
        public ActionResult ResourceDetails(string name)
        {
            var resource = GetResourceByName(name);
            return View(resource);
        }

        // Helper method to retrieve resource details
        private SupportResourceModel GetResourceByName(string name)
        {
            var resources = new List<SupportResourceModel>
            {
                new SupportResourceModel
                {
                    Name = "Befrienders Kenya (Mental Health Support)",
                    PhoneNumber = "+254 722 178 177",
                    Description = "A critical mental health support service providing confidential emotional support, " +
                                  "counseling, and suicide prevention assistance to individuals in Kenya.",
                    Website = "https://www.befrienderskenya.org/",
                    Category = "Mental Health",
                    AdditionalInfo = "Services are free, confidential, and available in multiple languages. " +
                                     "Trained counselors provide support for various mental health challenges, " +
                                     "including depression, anxiety, and suicide prevention."
                }
                // Add more detailed resources as needed
            };

            return resources.Find(r => r.Name == name) ?? new SupportResourceModel();
        }
    }
}