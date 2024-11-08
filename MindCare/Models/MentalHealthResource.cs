using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class MentalHealthResource
    {
        [Key]
        public int ResourceId { get; set; }
        public string? Description { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Type { get; set; } // Article, Video, Exercise

        public string? Content { get; set; }
        public string? Language { get; set; } // English, Swahili, etc.

        // Navigation Property for Many-to-Many Relationship
        public ICollection<ApplicationUser> Users { get; set; }

        public MentalHealthResource()
        {
            Users = new List<ApplicationUser>();
        }
    }
}
