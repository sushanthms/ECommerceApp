using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;// used to convert text to bytes
using System.Text.Json;// used to read the json payload sent by the razorpay

namespace ECommerceBackend.Services
{
    public class PaymentWebhookService
    {
        private readonly AppDbContext _context;

        public PaymentWebhookService(AppDbContext context)
        {
            _context = context;
        }

        public bool VerifySignature(string payload, string receivedSignature, string webhookSecret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);// creating HMAC-SHA256 cryptographic calculator

            var hashBytes = hmac.ComputeHash(payloadBytes);

            string computedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();// The calculated hash is converted into a hexadecimal string.

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(receivedSignature));
        }

        // processes the webhook after it has been received
        public async Task ProcessWebhookAsync(string payload)
        {
            // JsonDocument.Parse() converts the raw JSON string into a structure that C# can navigate/operate.
            using var document = JsonDocument.Parse(payload);
            // root represents the entire JSON object
            var root = document.RootElement;

            var eventType = root.GetProperty("event").GetString();// gets the value from "event": "payment.captured" and stores it in the variable called eventType

            if (eventType != "payment.captured" &&
                eventType != "payment.failed")
            {
                return;
            }

            var entity = root
                .GetProperty("payload")
                .GetProperty("payment")
                .GetProperty("entity");// entity is inside payment, and payment is inside payload.

            var razorpayOrderId =
                entity.GetProperty("order_id").GetString();

            var razorpayPaymentId =
                entity.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(razorpayOrderId))
                return;

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.RazorpayOrderId == razorpayOrderId);

            if (payment == null)
                return;

            if (eventType == "payment.captured")// from the razorpay
            {
                if (payment.Status == PaymentStatus.Captured)// status in the database and the status from the razorpay
                    return;

                payment.RazorpayPaymentId = razorpayPaymentId;
                payment.Status = PaymentStatus.Captured;// now the payment.Status will be less than captured in the database, so we change it to captured
                payment.UpdatedAt = DateTime.UtcNow;// captured means payment success

                var order = await _context.Orders
                    .FindAsync(payment.OrderId);

                if (order != null)
                {
                    order.PaymentStatus = "Paid";// here we change the status in Orders table
                }
            }
            else if (eventType == "payment.failed")
            {
                if (payment.Status == PaymentStatus.Captured)
                    return;

                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}