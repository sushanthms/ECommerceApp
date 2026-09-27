using ECommerceBackend.Data;
using ECommerceBackend.Payments.Models;// Gives this file access to everything inside that namespace: Payment class, PaymentStatus enum, and any future class
using Payment = ECommerceBackend.Payments.Models.Payment;// ECommerceBackend.Payments.Models.Payment — this is the full address of the Payment.cs
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;// RazorPay has Razorpay.Api.Payment, so we use alias here(using Payment)

namespace ECommerceBackend.Payments.Services
{
    public class PaymentService
    {
        private readonly RazorpayClient _client;// _client is the connection/interface to Razorpay's API.
        private readonly AppDbContext _context;// connection to our database through EF Core
        private readonly string _keySecret;

        public PaymentService(IConfiguration config, AppDbContext context)
        {
            var keyId = config["Razorpay:KeyId"]
                ?? throw new InvalidOperationException("Razorpay KeyId is missing.");

            _keySecret = config["Razorpay:KeySecret"]
                ?? throw new InvalidOperationException("Razorpay KeySecret is missing.");

            _client = new RazorpayClient(keyId, _keySecret);// creating RazorPay client. _client is now ready to communicate with Razorpay.
            _context = context;
        }

        // creates a Razorpay order for one of our existing orders. Task<Payment> means this method will return a Payment object
        public async Task<Payment> CreateRazorpayOrderAsync(int userId, int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>o.Id == orderId && o.UserId == userId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (order.PaymentStatus == "Paid")
                throw new InvalidOperationException("This order has already been paid.");

            // Reusing an existing pending Razorpay order instead of creating a duplicate
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.Id && p.Status == PaymentStatus.Created);

            if (existingPayment != null)
                return existingPayment;

            // checked means if the number is larger for a int to store it will give error. m means C# decimal literal.
            int amountInPaise = checked((int)Math.Round(order.TotalAmount * 100m, 0, MidpointRounding.AwayFromZero));
            // information that Razorpay needs.
            var options = new Dictionary<string, object>
            {
                ["amount"] = amountInPaise,
                ["currency"] = "INR",// We're telling Razorpay the currency is Indian Rupees.
                ["receipt"] = $"order_rcpt_{order.Id}"// Creates a receipt/reference value.
            };

            // Creating order in Razorpay by calling RazorPay
            var razorpayOrder = _client.Order.Create(options);// backend sends the data inside options to razorpay
            // Razorpay creates its own order and sends information(razorpay orderid) back.

            // Saving order payment information in our database
            var payment = new Payment
            {
                OrderId = order.Id,
                RazorpayOrderId =
                    razorpayOrder["id"]?.ToString()
                    ?? throw new InvalidOperationException("Razorpay did not return an order ID."),

                Amount = order.TotalAmount,
                Currency = "INR",
                Status = PaymentStatus.Created
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return payment;
        }

        public bool VerifySignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_order_id", razorpayOrderId },
                { "razorpay_payment_id", razorpayPaymentId },
                { "razorpay_signature", razorpaySignature },
                { "secret", _keySecret }
            };
            try
            {
                Utils.verifyPaymentSignature(attributes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task MarkPaymentCapturedAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.RazorpayOrderId == razorpayOrderId);

            if (payment == null)
                return;
            // at first time payment status will be created, we change it to captured. but we check if it captured because we may call MarkPaymentCapturedAsync again 
            if (payment.Status == PaymentStatus.Captured)
                return;

            payment.RazorpayPaymentId = razorpayPaymentId;
            payment.RazorpaySignature = razorpaySignature;
            payment.Status = PaymentStatus.Captured;// payment status is changed from created to captured
            payment.UpdatedAt = DateTime.UtcNow;

            // Here we use the OrderId stored in our Payment record to find the corresponding order in our Orders table.
            var order = await _context.Orders
                .FindAsync(payment.OrderId);

            if (order != null)
            {
                order.PaymentStatus = "Paid";// this is the status in Orders table
            }

            await _context.SaveChangesAsync();
        }
    }
}