namespace MindCare.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsModerated { get; set; }
        public virtual ApplicationUser Sender { get; set; }
    }
}
