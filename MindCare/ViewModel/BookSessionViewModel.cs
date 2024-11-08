using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class BookSessionViewModel
    {
        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time is required")]
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Please select a therapist")]
        [Display(Name = "Therapist")]
        public string TherapistId { get; set; }
    }
}