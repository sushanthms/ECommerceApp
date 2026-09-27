using ECommerceBackend.Models;

namespace ECommerceBackend.Payments.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string RazorpayOrderId { get; set; } = null!;
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";

        public PaymentStatus Status { get; set; } = PaymentStatus.Created;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum PaymentStatus
    {
        Created,
        Captured,
        Failed,
        Refunded
    }
}