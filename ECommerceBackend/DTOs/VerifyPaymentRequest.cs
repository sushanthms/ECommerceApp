namespace ECommerceBackend.DTOs
{
    public class VerifyPaymentRequest
    {
        public string RazorpayOrderId { get; set; } = null!;
        public string RazorpayPaymentId { get; set; } = null!;
        public string RazorpaySignature { get; set; } = null!;

        public CreateOrderDto Order { get; set; } = new();
    }
}