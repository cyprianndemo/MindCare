using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class PaymentViewModel
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public string StripeToken { get; set; }
        public string LastFourDigits { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
/*        public int AppointmentId { get; set; }
*/        public string MPesaTransactionCode { get; set; }

        public string StripeChargeId { get; set; }

        // Add these properties for Visa payment
        public string CardName { get; set; }

        public string CardNumber { get; set; }

        public string ExpiryDate { get; set; } // Format: MM/YY

        public string Cvv { get; set; }
        public bool IsSuccessful { get; set; }
        public string? TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
