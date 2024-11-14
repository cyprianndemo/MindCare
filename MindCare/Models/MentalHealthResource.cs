using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MindCare.Models
{
    public class MentalHealthResource
    {
        [Key]
        public int ResourceId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Type { get; set; } // Article, Video, Exercise

        public string? Description { get; set; }
        public string? Content { get; set; }
        public string? Language { get; set; } // English, Swahili, etc.

        // New Category Property to categorize resources
        [Required]
        public string? Category { get; set; } // e.g., "Awareness", "Support", "Stigma"

        // New Url Property to store resource link
        public string? Url { get; set; } // Link to the resource (e.g., article, video)

        // Navigation Property for Many-to-Many Relationship
        public ICollection<ApplicationUser> Users { get; set; }

        public MentalHealthResource()
        {
            Users = new List<ApplicationUser>();
        }
    }
}
