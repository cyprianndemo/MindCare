using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class AppointmentApprovalViewModel
    {
        public int AppointmentId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        [Required(ErrorMessage = "Please provide notes for the approval")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string PsychiatristNotes { get; set; }
        public string TherapistNotes { get; set; }

    }

}
