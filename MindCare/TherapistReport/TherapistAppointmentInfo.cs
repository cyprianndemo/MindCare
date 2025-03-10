namespace MindCare.TherapistReport
{
    public class TherapistReportViewModel
    {
        public string TherapistName { get; set; }
        public int TotalSessions { get; set; }
        public int PendingSessions { get; set; }
        public int ApprovedSessions { get; set; }
        public List<TherapistAppointmentInfo> UpcomingPendingAppointments { get; set; }
        public List<TherapistAppointmentInfo> UpcomingApprovedAppointments { get; set; }
    }

    public class TherapistAppointmentInfo
    {
        public int AppointmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string Status { get; set; }
    }
}
