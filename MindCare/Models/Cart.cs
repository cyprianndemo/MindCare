using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        // Foreign Key
        [ForeignKey("Student")]
        public string? StudentId { get; set; }
        public Student Student { get; set; } // Student who owns the cart

        // Navigation Property
        public ICollection<CartItem> CartItems { get; set; } // Items in the cart

        public Cart()
        {
            CartItems = new List<CartItem>();
        }
    }
}
