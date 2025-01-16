using MindCare.Models;

namespace MindCare.ViewModel
{
    public class PatientViewModel
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime LastAppointment { get; set; }
        public int TotalAppointments { get; set; }
       
    }
}
