using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class FeedbackCreateViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        public string Content { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        // For display purposes
        public string TherapistName { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
