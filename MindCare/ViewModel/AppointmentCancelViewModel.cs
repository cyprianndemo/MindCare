using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class AppointmentCancelViewModel
    {
        public int AppointmentId { get; set; }
        public string StudentName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Please provide a reason for cancellation")]
        [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters")]
        public string CancellationReason { get; set; }
    }
}
