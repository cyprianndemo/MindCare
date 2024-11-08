namespace MindCare.Models
{
    public class MPesaPayment : Payment
    {
        public string TransactionCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
