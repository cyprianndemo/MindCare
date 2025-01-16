namespace MindCare.ViewModel
{
    public class ReceiptViewModel
    {
        public string ReceiptNumber { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
    }

    public class CustomerDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
