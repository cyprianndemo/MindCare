using Microsoft.AspNetCore.Mvc;
using MindCare.MentalResources;
using MentalHealthResource = MindCare.MentalResources.MentalHealthResource;

namespace MindCare.Controllers
{
    public class ResourcesController : Controller
    {
        // GET: Resources
        public ActionResult Index()
        {
            var resourceCategories = new List<ResourceCategory>
            {
                new ResourceCategory
                {
                    Id = 1,
                    Name = "Articles",
                    Description = "Informational articles on various mental health topics",
                    IconClass = "fa-book-open"
                },
                new ResourceCategory
                {
                    Id = 2,
                    Name = "Exercises",
                    Description = "Practical exercises to improve mental wellbeing",
                    IconClass = "fa-dumbbell"
                },
                new ResourceCategory
                {
                    Id = 3,
                    Name = "Professional Help",
                    Description = "Information about therapy, psychiatry, and other professional services",
                    IconClass = "fa-user-md"
                },
                new ResourceCategory
                {
                    Id = 4,
                    Name = "Self-Help Tools",
                    Description = "Apps, worksheets, and other tools for self-management",
                    IconClass = "fa-tools"
                }
            };

            return View(resourceCategories);
        }

        // GET: Resources/Category/1
        public ActionResult Category(int id)
        {
            var resources = GetResourcesByCategory(id);
            var categoryName = GetCategoryName(id);

            ViewBag.CategoryName = categoryName;
            ViewBag.CategoryId = id;

            return View(resources);
        }

        // GET: Resources/Details/5
        public ActionResult Details(int id)
        {
            var resource = GetResourceById(id);
            

            return View(resource);
        }

        // GET: Resources/WhySeekHelp
        public ActionResult WhySeekHelp()
        {
            var reasons = new List<TherapyReason>
            {
                new TherapyReason
                {
                    Title = "Professional Guidance",
                    Description = "Mental health professionals are trained to help identify and treat specific mental health conditions using evidence-based approaches.",
                    IconClass = "fa-compass"
                },
                new TherapyReason
                {
                    Title = "Objective Perspective",
                    Description = "Therapists and psychiatrists provide an unbiased viewpoint that friends and family may not be able to offer.",
                    IconClass = "fa-balance-scale"
                },
                new TherapyReason
                {
                    Title = "Safe Environment",
                    Description = "Therapy provides a confidential and non-judgmental space to express feelings and work through challenges.",
                    IconClass = "fa-shield-alt"
                },
                new TherapyReason
                {
                    Title = "Skill Development",
                    Description = "Learn practical coping strategies and techniques to manage symptoms and improve quality of life.",
                    IconClass = "fa-tools"
                },
                new TherapyReason
                {
                    Title = "Medical Treatment",
                    Description = "Psychiatrists can prescribe medications when needed as part of a comprehensive treatment plan.",
                    IconClass = "fa-prescription-bottle-alt"
                },
                new TherapyReason
                {
                    Title = "Prevention and Maintenance",
                    Description = "Regular therapy can help prevent minor issues from developing into more serious problems.",
                    IconClass = "fa-heart-circle"
                }
            };

            return View(reasons);
        }

        // Helper methods to simulate database access
        private List<MentalHealthResource> GetResourcesByCategory(int categoryId)
        {
            // In a real application, this would come from a database
            var allResources = GetAllResources();
            return allResources.Where(r => r.CategoryId == categoryId).ToList();
        }

        private string GetCategoryName(int categoryId)
        {
            switch (categoryId)
            {
                case 1: return "Articles";
                case 2: return "Exercises";
                case 3: return "Professional Help";
                case 4: return "Self-Help Tools";
                default: return "Resources";
            }
        }

        private MentalHealthResource GetResourceById(int id)
        {
            var allResources = GetAllResources();
            return allResources.FirstOrDefault(r => r.Id == id);
        }

        private List<MentalHealthResource> GetAllResources()
        {
            // This would typically come from a database
            return new List<MentalHealthResource>
            {
                // Articles
                new MentalHealthResource
                {
                    Id = 1,
                    CategoryId = 1,
                    Title = "Understanding Depression",
                    Summary = "Learn about the symptoms, causes, and treatments for depression",
                    Content = "Depression is more than just feeling sad. It's a serious mental health condition that affects how you feel, think, and handle daily activities...",
                    ImageUrl = "/images/depression.jpeg",
                    Tags = new List<string> { "depression", "mood disorders", "mental health basics" }
                },
                new MentalHealthResource
                {
                    Id = 2,
                    CategoryId = 1,
                    Title = "Managing Anxiety in Daily Life",
                    Summary = "Practical tips for coping with anxiety symptoms",
                    Content = "Anxiety is a normal response to stress, but when it becomes excessive, it can interfere with your daily activities...",
                    ImageUrl = "/images/anxiety.png",
                    Tags = new List<string> { "anxiety", "stress management", "coping skills" }
                },
                new MentalHealthResource
                {
                    Id = 3,
                    CategoryId = 1,
                    Title = "The Importance of Sleep for Mental Health",
                    Summary = "How sleep affects your mental wellbeing and tips for better sleep",
                    Content = "Sleep plays a vital role in good health and well-being throughout your life. Getting enough quality sleep can help protect your mental health...",
                    ImageUrl = "/images/sleep.jpeg",
                    Tags = new List<string> { "sleep", "self-care", "wellness" }
                },
                
                // Exercises
                new MentalHealthResource
                {
                    Id = 4,
                    CategoryId = 2,
                    Title = "5-Minute Mindfulness Meditation",
                    Summary = "A quick meditation exercise you can do anywhere",
                    Content = "This simple mindfulness exercise can help center your thoughts and reduce stress in just 5 minutes...",
                    ImageUrl = "/images/meditation.jpeg",
                    Tags = new List<string> { "meditation", "mindfulness", "stress reduction" }
                },
                new MentalHealthResource
                {
                    Id = 5,
                    CategoryId = 2,
                    Title = "Progressive Muscle Relaxation",
                    Summary = "Learn to release tension throughout your body",
                    Content = "Progressive muscle relaxation is a deep relaxation technique that has been effectively used to control stress and anxiety...",
                    ImageUrl = "/images/relaxation.jpeg",
                    Tags = new List<string> { "relaxation", "anxiety", "physical techniques" }
                },
                new MentalHealthResource
                {
                    Id = 6,
                    CategoryId = 2,
                    Title = "Gratitude Journaling Exercise",
                    Summary = "Improve your outlook through gratitude practice",
                    Content = "Regularly writing down things you're grateful for can improve your mental health and increase positive emotions...",
                    ImageUrl = "/images/gratitude.jpeg",
                    Tags = new List<string> { "journaling", "gratitude", "positive psychology" }
                },
                
                // Professional Help
                new MentalHealthResource
                {
                    Id = 7,
                    CategoryId = 3,
                    Title = "Types of Mental Health Professionals",
                    Summary = "Understanding the different roles in mental healthcare",
                    Content = "There are many types of mental health professionals. Understanding the differences can help you find the right support...",
                    ImageUrl = "/images/professionals.jpeg",
                    Tags = new List<string> { "therapy", "psychiatry", "professional help" }
                },
                new MentalHealthResource
                {
                    Id = 8,
                    CategoryId = 3,
                    Title = "What to Expect in Your First Therapy Session",
                    Summary = "Preparing for your first appointment with a therapist",
                    Content = "Starting therapy can feel intimidating. Knowing what to expect can help ease anxiety about your first session...",
                    ImageUrl = "/images/therapy.jpeg",
                    Tags = new List<string> { "therapy", "getting started", "mental health treatment" }
                },
                new MentalHealthResource
                {
                    Id = 9,
                    CategoryId = 3,
                    Title = "Finding Affordable Mental Healthcare",
                    Summary = "Resources for accessing care on any budget",
                    Content = "Cost shouldn't be a barrier to mental healthcare. Here are options for finding affordable support...",
                    ImageUrl = "/images/affordable-care.jpeg",
                    Tags = new List<string> { "healthcare access", "affordability", "resources" }
                },
                
                // Self-Help Tools
                new MentalHealthResource
                {
                    Id = 10,
                    CategoryId = 4,
                    Title = "Recommended Mental Health Apps",
                    Summary = "Mobile applications to support your mental wellbeing",
                    Content = "These evidence-based mental health apps can help you track moods, practice mindfulness, and develop coping skills...",
                    ImageUrl = "/images/mindcare logo.webp",
                    Tags = new List<string> { "technology", "self-help", "apps" }
                },
                new MentalHealthResource
                {
                    Id = 11,
                    CategoryId = 4,
                    Title = "Printable CBT Worksheets",
                    Summary = "Cognitive-behavioral therapy exercises you can do at home",
                    Content = "These worksheets based on cognitive-behavioral therapy techniques can help you identify and challenge negative thought patterns...",
                    ImageUrl = "/images/CBT.jpeg",
                    Tags = new List<string> { "CBT", "worksheets", "thought patterns" }
                },
                new MentalHealthResource
                {
                    Id = 12,
                    CategoryId = 4,
                    Title = "Creating a Wellness Plan",
                    Summary = "Design your personal strategy for maintaining mental health",
                    Content = "A wellness plan can help you identify triggers, recognize warning signs, and develop strategies to maintain good mental health...",
                    ImageUrl = "/images/wellness-planning.png",
                    Tags = new List<string> { "planning", "self-care", "wellness" }
                }
            };
        }
    }
}