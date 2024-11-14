using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class AppointmentEditViewModel
    {
        public int AppointmentId { get; set; }
        public string StudentName { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; }
    }
}
