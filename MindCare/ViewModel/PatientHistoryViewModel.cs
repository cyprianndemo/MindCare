namespace MindCare.ViewModel
{
    public class PatientHistoryViewModel
    {
        public string StudentId { get; set; }
        public string PatientName { get; set; }
        public string Email { get; set; }
        public List<AppointmentHistoryViewModel> Appointments { get; set; }
    }

    public class AppointmentHistoryViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CancellationTime { get; set; }
    }

}
