using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class Payment
    {
        [Key]
        public int? PaymentId { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }   
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus? Status { get; set; }
        public PaymentMethod? Method { get; set; }
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? Student { get; set; }
        public int? AppointmentId { get; set; }

        public string? TransactionCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MpesaReceiptNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? MerchantRequestID { get; set; }
        public string? CheckoutRequestID { get; set; }
        public bool IsSuccessful { get; set; }

    }
}
