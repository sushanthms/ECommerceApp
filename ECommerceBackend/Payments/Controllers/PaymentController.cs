using ECommerceBackend.Payments.DTOs;
using ECommerceBackend.Payments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceBackend.Payments.Controllers
{
    [ApiController]
    [Route("api/payment")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly IConfiguration _config;

        public PaymentController(PaymentService paymentService, IConfiguration config)
        {
            _paymentService = paymentService;
            _config = config;
        }

        [HttpPost("create/{orderId}")]
        public async Task<IActionResult> Create(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var payment = await _paymentService.CreateRazorpayOrderAsync(userId, orderId);

            return Ok(new CreatePaymentResponse
            {
                RazorpayOrderId = payment.RazorpayOrderId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                KeyId = _config["Razorpay:KeyId"]!
            });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest request)
        {
            bool isValid = _paymentService.VerifySignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

            if (!isValid)
                return BadRequest(new
                {
                    message = "Payment verification failed"
                });

            await _paymentService.MarkPaymentCapturedAsync(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

            return Ok(new
            {
                message = "Payment verified successfully"
            });
        }
    }
}