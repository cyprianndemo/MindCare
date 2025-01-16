using MindCare.Models;

namespace MindCare.Data
{
    public static class ResourceSeedData
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            if (context.MentalHealthResources.Any())
            {
                return;
            }

            var resources = new List<MentalHealthResource>
        {
            new MentalHealthResource
            {
                Title = "Immediate Crisis Support Guide",
                Description = "Step-by-step guide for managing mental health crises",
                Content = "Detailed content about crisis management steps...",
                Category = "Crisis",
                ResourceType = "Emergency Contacts",
                Scenario = "Crisis",
                Urgency = "Immediate",
                IsDownloadable = true,
                FileUrl = "crisis-guide.pdf",
                Tags = new List<string> { "emergency", "crisis", "immediate help" }
            },
            new MentalHealthResource
            {
                Title = "Anxiety Management Techniques",
                Description = "Learn effective techniques for managing anxiety",
                Content = "Various anxiety management strategies...",
                Category = "Anxiety",
                ResourceType = "Self-Help Guides",
                Scenario = "Daily Management",
                Urgency = "Soon",
                IsDownloadable = true,
                FileUrl = "anxiety-techniques.pdf",
                Tags = new List<string> { "anxiety", "self-help", "coping" }
            },
            new MentalHealthResource
            {
                Title = "Depression Recovery Workbook",
                Description = "Interactive workbook for managing depression",
                Content = "Exercises and worksheets for depression management...",
                Category = "Depression",
                ResourceType = "Interactive Tools",
                Scenario = "Recovery",
                Urgency = "General",
                IsDownloadable = true,
                FileUrl = "depression-workbook.pdf",
                Tags = new List<string> { "depression", "workbook", "recovery" }
            },
            new MentalHealthResource
            {
                Title = "Stress Management at Work",
                Description = "Guide to managing workplace stress",
                Category = "Stress",
                ResourceType = "Professional Resources",
                Scenario = "Daily Management",
                Urgency = "General",
                Content = "Strategies for workplace stress management...",
                IsDownloadable = true,
                FileUrl = "workplace-stress.pdf",
                Tags = new List<string> { "stress", "work", "professional" }
            },
            new MentalHealthResource
            {
                Title = "Building Healthy Relationships",
                Description = "Guide to maintaining healthy relationships",
                Category = "Relationships",
                ResourceType = "Educational Materials",
                Scenario = "Prevention",
                Urgency = "General",
                Content = "Tips and strategies for healthy relationships...",
                IsDownloadable = true,
                FileUrl = "relationships-guide.pdf",
                Tags = new List<string> { "relationships", "communication", "support" }
            }
            // Add more resources as needed
        };

            await context.MentalHealthResources.AddRangeAsync(resources);
            await context.SaveChangesAsync();
        }
    }
}
