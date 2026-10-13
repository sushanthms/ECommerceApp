using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Models;
using ECommerceBackend.Logging;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;// RazorPay has Razorpay.Api.Payment, so we use alias here(using Payment)
using Order = ECommerceBackend.Models.Order;
using Payment = ECommerceBackend.Models.Payment;

namespace ECommerceBackend.Services
{
    public class PaymentService
    {
        private readonly RazorpayClient _client;// _client is the connection/interface to Razorpay's API.
        private readonly AppDbContext _context;// connection to our database through EF Core
        private readonly OrderService _orderService;
        private readonly IApplicationLogger _logger;
        private readonly string _keySecret;

        public PaymentService(IConfiguration config, AppDbContext context, IApplicationLogger logger, OrderService orderService)
        {
            var keyId = config["Razorpay:KeyId"]
                ?? throw new InvalidOperationException("Razorpay KeyId is missing.");

            _keySecret = config["Razorpay:KeySecret"]
                ?? throw new InvalidOperationException("Razorpay KeySecret is missing.");

            _client = new RazorpayClient(keyId, _keySecret);// creating RazorPay client. _client is now ready to communicate with Razorpay.

            _context = context;
            _logger = logger;
            _orderService = orderService;
        }

        public async Task<Payment> CreateRazorpayOrderAsync(int userId, CreateOrderDto dto)
        {
            await _orderService.GetAddressAsync(userId, dto);

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            var unavailableItems = cartItems
                .Where(c => c.Product.IsDeleted)
                .Select(c => c.Product.Name)
                .ToList();

            if (unavailableItems.Count > 0)
                throw new InvalidOperationException("Some products in your cart are no longer available.");

            decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);

            int amountInPaise = checked((int)Math.Round(totalAmount * 100m, 0, MidpointRounding.AwayFromZero));

            var options = new Dictionary<string, object>
            {
                ["amount"] = amountInPaise,
                ["currency"] = "INR",
                ["receipt"] = $"payment_rcpt_{Guid.NewGuid()}"
            };

            var razorpayOrder = _client.Order.Create(options);

            var payment = new Payment
            {
                UserId = userId,
                OrderId = null,

                RazorpayOrderId =
                    razorpayOrder["id"]?.ToString()
                    ?? throw new InvalidOperationException(
                        "Razorpay did not return an order ID."
                    ),

                Amount = totalAmount,
                Currency = "INR",
                Status = PaymentStatus.Created
            };

            _context.Payments.Add(payment);
            await _logger.LogMessageAsync($"RazorPay order is created for userid: {userId}", "Information");

            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<bool> VerifySignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
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
                await _logger.LogMessageAsync($"Payment verified for paymentID : {razorpayPaymentId}", "Information");
                return true;
            }
            catch
            {
                await _logger.LogMessageAsync($"Payment verification failed for paymentID: {razorpayPaymentId}", "Information");
                return false;
            }
        }

        public async Task<int> MarkPaymentCapturedAsync(int userId, CreateOrderDto dto, string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.RazorpayOrderId == razorpayOrderId && p.UserId == userId);

            if (payment == null)
                throw new KeyNotFoundException("Payment not found.");

            // at first time payment status will be created, we change it to captured. but we check if it captured because we may call MarkPaymentCapturedAsync again 
            if (payment.Status == PaymentStatus.Captured)
                throw new InvalidOperationException("Payment has already been processed.");

            var address = await _orderService.GetAddressAsync(userId, dto);

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            decimal cartTotal = cartItems.Sum(c => c.Product.Price * c.Quantity);

            if (cartTotal != payment.Amount)
                throw new InvalidOperationException("Cart changed after payment started.");

            var unavailableItems = cartItems
                .Where(c => c.Product.IsDeleted)
                .ToList();

            if (unavailableItems.Count > 0)
                throw new InvalidOperationException("Some products in your cart are no longer available.");

            var order = new Order
            {
                UserId = userId,

                DeliveryName = address.FullName,
                Phone = address.Phone,
                Address = string.Join(", ", new[] { address.Line1, address.Line2 }
                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                City = address.City,
                State = address.State,
                Pincode = address.Pincode,

                TotalAmount = payment.Amount,
                Status = "Pending",

                PaymentMethod = "Online Payment",
                PaymentStatus = "Paid",

                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                };

                _context.OrderItems.Add(orderItem);
            }

            _context.CartItems.RemoveRange(cartItems);

            payment.Order = order;
            payment.RazorpayPaymentId = razorpayPaymentId;
            payment.RazorpaySignature = razorpaySignature;
            payment.Status = PaymentStatus.Captured;// payment status is changed from created to captured
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (dto.AddressId == null && dto.SaveAddress)
            {
                try
                {
                    bool isFirst = !await _context.Addresses.AnyAsync(a => a.UserId == userId);

                    _context.Addresses.Add(new Models.Address
                    {
                        UserId = userId,
                        FullName = address.FullName,
                        Phone = address.Phone,
                        Line1 = address.Line1,
                        City = address.City,
                        State = address.State,
                        Pincode = address.Pincode,
                        IsDefault = isFirst
                    });

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await _logger.LogMessageAsync($"Saving new address failed - UserId: {userId}, Error: {ex.Message}", "Warning");
                }
            }

            await _logger.LogMessageAsync($"Payment Captured and Order Placed for userId: {userId}", "Information");

            return order.Id;
        }
    }
}