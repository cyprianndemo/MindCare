using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class MoodEntry
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int MoodScore { get; set; }

        public string? Notes { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        public string? StudentId { get; set; }

        [Required(ErrorMessage = "Please select a mood")]
        public string? Mood { get; set; }

        [Required(ErrorMessage = "Please select an intensity level")]
        [Range(1, 10, ErrorMessage = "Intensity must be between 1 and 10")]
        public int Intensity { get; set; }

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.Now;
    }
}