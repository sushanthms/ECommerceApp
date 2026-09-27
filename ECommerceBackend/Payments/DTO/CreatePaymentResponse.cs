namespace ECommerceBackend.Payments.DTOs
{
    public class CreatePaymentResponse
    {
        public string RazorpayOrderId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string KeyId { get; set; } = null!;
    }
}