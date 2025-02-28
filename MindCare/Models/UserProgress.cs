namespace MindCare.Models
{
    public class UserProgress
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ResourceId { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

}
