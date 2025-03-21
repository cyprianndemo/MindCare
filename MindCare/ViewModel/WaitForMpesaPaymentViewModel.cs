namespace MindCare.ViewModel
{
    public class WaitForMpesaPaymentViewModel
    {
        public string TransactionId { get; set; }
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
    }
}
