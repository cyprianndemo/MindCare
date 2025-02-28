// Models/Feedback.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public string? Content { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [ForeignKey("Student")]
        public string? StudentId { get; set; }
        public ApplicationUser? Student { get; set; }

        [ForeignKey("Therapist")]
        public string? TherapistId { get; set; }
        public ApplicationUser? Therapist { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to link with appointment
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
    }
}