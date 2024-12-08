namespace MindCare.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } // e.g., "Appointment", "System", etc.
        public virtual ApplicationUser User { get; set; }
    }
}
