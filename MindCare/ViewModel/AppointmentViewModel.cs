namespace MindCare.ViewModel
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string StudentId { get; set; }
        public string StudentEmail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentName { get; set; }

        public string TherapistId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string StudentFullName => $"{StudentFirstName} {StudentLastName}";

        public DateTime CreatedAt { get; set; }
    }
}
