using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindCare.Models
{
    public class MoodEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a mood")]
        public string Mood { get; set; }

        [Required(ErrorMessage = "Please select an intensity level")]
        [Range(1, 10, ErrorMessage = "Intensity must be between 1 and 10")]
        public int Intensity { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public DateTime EntryDate { get; set; }
    }
}