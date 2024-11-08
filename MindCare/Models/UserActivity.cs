using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindCare.Models
{
    public class UserActivity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Action { get; set; } // E.g., "Logged In", "Viewed Report", "Updated Settings"

        public string Description { get; set; } // Additional details about the action

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation property for linking with the user
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
