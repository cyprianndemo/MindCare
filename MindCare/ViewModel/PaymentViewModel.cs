using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class PaymentViewModel
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string TenantId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public string StripeToken { get; set; }
        public string LastFourDigits { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        public string MPesaTransactionCode { get; set; }

        [Required]
        public int UnitId { get; set; }

        public string PropertyName { get; set; }
        public string UnitNumber { get; set; }
        public string StripeChargeId { get; set; }

        // Add these properties for Visa payment
        public string CardName { get; set; }

        public string CardNumber { get; set; }

        public string ExpiryDate { get; set; } // Format: MM/YY

        public string Cvv { get; set; }
    }
}
