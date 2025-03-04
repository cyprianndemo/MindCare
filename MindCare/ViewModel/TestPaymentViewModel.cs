using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class TestPaymentViewModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string PhoneNumber { get; set; }
    }
}