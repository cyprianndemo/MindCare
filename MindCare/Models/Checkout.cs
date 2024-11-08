using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Checkout
    {
        [Key]
        public int CheckoutId { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        // Foreign Key to link checkout with Cart
        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        // Foreign Key to link checkout with Student
        [ForeignKey("Student")]
        public string? StudentId { get; set; }
        public Student? Student { get; set; }

        // Payment Status
        public string? Status { get; set; } // e.g., Pending, Paid, Completed
    }
}
