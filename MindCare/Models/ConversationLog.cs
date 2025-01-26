namespace MindCare.Models
{
    public class ConversationLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserMessage { get; set; }
        public string BotResponse { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
