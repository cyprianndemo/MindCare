using MindCare.Models;

namespace MindCare.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any resources
            if (context.MentalHealthResources.Any())
            {
                return;   // DB has been seeded
            }

            var resources = new MentalHealthResource[]
            {
            new MentalHealthResource
            {
                Title = "Understanding Anxiety: A Comprehensive Guide",
                Description = "Learn about the symptoms, causes, and treatments for anxiety disorders.",
                Content = "<h2>What is Anxiety?</h2><p>Anxiety is your body's natural response to stress. It's a feeling of fear or apprehension about what's to come. The first day of school, going to a job interview, or giving a speech may cause most people to feel fearful and nervous.</p><h2>Symptoms of Anxiety</h2><p>Anxiety can manifest in many ways, including:</p><ul><li>Increased heart rate</li><li>Rapid breathing</li><li>Restlessness</li><li>Trouble concentrating</li><li>Difficulty falling asleep</li></ul><h2>Coping Strategies</h2><p>There are many proven strategies to help manage anxiety, including deep breathing exercises, physical activity, and cognitive behavioral therapy techniques.</p>",
                Category = "Anxiety",
                ResourceType = "Guide",
                Tags = new List<string> { "anxiety", "mental health", "self-help", "stress management" },
                DateAdded = DateTime.Now,
                ViewCount = 0,
                IsDownloadable = false
            },
            new MentalHealthResource
            {
                Title = "Mindfulness Meditation for Beginners",
                Description = "A simple introduction to mindfulness practices that can reduce stress and improve well-being.",
                Content = "<h2>What is Mindfulness?</h2><p>Mindfulness is the practice of purposely focusing your attention on the present moment—and accepting it without judgment. Mindfulness is now being examined scientifically and has been found to be a key element in stress reduction and overall happiness.</p><h2>Basic Mindfulness Meditation</h2><p>Here's a simple exercise to try:</p><ol><li>Sit on a straight-backed chair or cross-legged on the floor.</li><li>Focus on an aspect of your breathing, such as the sensations of air flowing into your nostrils and out of your mouth, or your belly rising and falling as you inhale and exhale.</li><li>Once your mind wanders, return your focus back to your breath.</li></ol>",
                Category = "Stress Management",
                ResourceType = "Exercise",
                Tags = new List<string> { "mindfulness", "meditation", "stress reduction", "relaxation" },
                DateAdded = DateTime.Now,
                ViewCount = 0,
                IsDownloadable = false
            },
            new MentalHealthResource
            {
                Title = "Depression: Signs, Symptoms, and Treatment Options",
                Description = "Information about recognizing depression and effective treatment approaches.",
                Content = "<h2>Understanding Depression</h2><p>Depression is more than just feeling sad. It's a mood disorder that causes a persistent feeling of sadness and loss of interest. It affects how you feel, think and behave and can lead to a variety of emotional and physical problems.</p><h2>Common Symptoms</h2><p>Depression symptoms can vary from mild to severe and may include:</p><ul><li>Feeling sad or having a depressed mood</li><li>Loss of interest or pleasure in activities once enjoyed</li><li>Changes in appetite</li><li>Trouble sleeping or sleeping too much</li><li>Loss of energy or increased fatigue</li><li>Feeling worthless or guilty</li><li>Difficulty thinking, concentrating or making decisions</li><li>Thoughts of death or suicide</li></ul><h2>Treatment Approaches</h2><p>Depression is among the most treatable of mental disorders. Between 80% and 90% of people with depression eventually respond well to treatment.</p>",
                Category = "Depression",
                ResourceType = "Guide",
                Tags = new List<string> { "depression", "mental health", "treatment", "symptoms" },
                DateAdded = DateTime.Now,
                ViewCount = 0,
                IsDownloadable = false
            }
            };

            foreach (MentalHealthResource r in resources)
            {
                context.MentalHealthResources.Add(r);
            }

            context.SaveChanges();
        }
    }
}
