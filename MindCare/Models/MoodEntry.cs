namespace MindCare.Models
{
    public class MoodEntry
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int MoodScore { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
    }
}
