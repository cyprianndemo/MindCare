using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class MentalHealthResource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Content { get; set; }

        [Required]
        public string Category { get; set; }  // Depression, Anxiety, Stress, etc.

        [Required]
        public string ResourceType { get; set; }  // Self-Help, Professional Help, Emergency, etc.

        [Required]
        public string Scenario { get; set; }  // Crisis, Daily Management, Prevention, etc.

        public string Urgency { get; set; }  // Immediate, Soon, General

        public string Url { get; set; }
        public int ViewCount { get; set; }
        public string FileUrl { get; set; }

        public bool IsDownloadable { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }

    public class ResourceViewModel
    {
        public IEnumerable<MentalHealthResource> Resources { get; set; }
        public string SelectedCategory { get; set; }
        public string SelectedType { get; set; }
        public string SelectedScenario { get; set; }
        public IEnumerable<MentalHealthResource> RecommendedResources { get; set; }
        public string SearchTerm { get; set; }
        public int ViewCount { get; set; }
        public List<string> Categories { get; set; }
        public List<string> ResourceTypes { get; set; }
        public List<string> Scenarios { get; set; }
        public Dictionary<string, string> ScenarioDescriptions { get; set; }
    }

    public static class ResourceConstants
    {
        public static readonly Dictionary<string, string[]> Categories = new()
        {
            { "Depression", new[] { "Mild", "Moderate", "Severe" } },
            { "Anxiety", new[] { "General", "Social", "Panic", "PTSD" } },
            { "Stress", new[] { "Work", "Academic", "Relationship", "Financial" } },
            { "Crisis", new[] { "Suicidal Thoughts", "Emotional Crisis", "Trauma" } },
            { "Relationships", new[] { "Family", "Romantic", "Workplace", "Social" } },
            { "Self-Care", new[] { "Mindfulness", "Exercise", "Nutrition", "Sleep" } }
        };

        public static readonly Dictionary<string, string> Scenarios = new()
        {
            { "Crisis", "Immediate support for urgent mental health situations" },
            { "Daily Management", "Tools and techniques for day-to-day mental health" },
            { "Prevention", "Resources to maintain mental wellness and prevent issues" },
            { "Recovery", "Support for healing and building resilience" },
            { "Education", "Understanding mental health conditions and treatments" },
            { "Support Network", "Building and maintaining supportive relationships" }
        };

        public static readonly Dictionary<string, string> ResourceTypes = new()
        {
            { "Self-Help Guides", "Downloadable guides for personal development" },
            { "Professional Resources", "Materials from mental health professionals" },
            { "Interactive Tools", "Online exercises and assessments" },
            { "Emergency Contacts", "Crisis hotlines and immediate help" },
            { "Community Support", "Group resources and peer support" },
            { "Educational Materials", "Learning resources about mental health" }
        };
    }
}