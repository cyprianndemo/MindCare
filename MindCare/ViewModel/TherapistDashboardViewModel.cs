namespace MindCare.ViewModel
{
    public class TherapistDashboardViewModel
    {
        public string TherapistName { get; set; }
        public List<AppointmentViewModel> UpcomingAppointments { get; set; }
        public List<AppointmentViewModel> TodaysAppointments { get; set; }
        public int PendingAppointmentsCount { get; set; }
    }
}
