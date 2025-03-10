using MindCare.Models;

namespace MindCare.ViewModel
{
    public class StudentReportViewModel
    {
        // Student information
        public ApplicationUser Student { get; set; }
        public string StudentName { get; set; }
        public DateTime ReportGeneratedDate { get; set; }

        // Therapy statistics
        public int TotalTherapySessions { get; set; }
        public int PendingTherapySessions { get; set; }
        public int ApprovedTherapySessions { get; set; }
        public int CompletedTherapySessions { get; set; }
        public int CancelledTherapySessions { get; set; }

        // Psychiatry statistics
        public int TotalPsychiatrySessions { get; set; }
        public int PendingPsychiatrySessions { get; set; }
        public int ApprovedPsychiatrySessions { get; set; }
        public int CompletedPsychiatrySessions { get; set; }
        public int CancelledPsychiatrySessions { get; set; }

        // Upcoming appointments
        public List<Appointment> UpcomingTherapyAppointments { get; set; }
        public List<Appointment> UpcomingPsychiatryAppointments { get; set; }

        // Recent appointments
        public List<Appointment> RecentTherapyAppointments { get; set; }
        public List<Appointment> RecentPsychiatryAppointments { get; set; }

        // Prescriptions
        public List<Prescription> Prescriptions { get; set; }
        public List<Prescription> ActivePrescriptions { get; set; }
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptionsCount { get; set; }
        public int InactivePrescriptionsCount { get; set; }

        // Lookup dictionaries for provider names
        public Dictionary<string, string> TherapistNames { get; set; }
        public Dictionary<string, string> PsychiatristNames { get; set; }

        // Constructor
        public StudentReportViewModel()
        {
            UpcomingTherapyAppointments = new List<Appointment>();
            UpcomingPsychiatryAppointments = new List<Appointment>();
            RecentTherapyAppointments = new List<Appointment>();
            RecentPsychiatryAppointments = new List<Appointment>();
            Prescriptions = new List<Prescription>();
            ActivePrescriptions = new List<Prescription>();
            TherapistNames = new Dictionary<string, string>();
            PsychiatristNames = new Dictionary<string, string>();
        }
    }
}