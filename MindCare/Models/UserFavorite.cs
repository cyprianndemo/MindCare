namespace MindCare.Models
{
    public class UserFavorite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ResourceId { get; set; }
        public DateTime DateAdded { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

}
