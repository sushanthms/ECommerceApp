using ECommerceBackend.DTOs;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/payment")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly PaymentWebhookService _webhookService;
        private readonly IConfiguration _config;

        public PaymentController(PaymentService paymentService, PaymentWebhookService webhookService, IConfiguration config)
        {
            _paymentService = paymentService;
            _webhookService = webhookService;
            _config = config;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var payment = await _paymentService.CreateRazorpayOrderAsync(userId, dto);

                return Ok(new CreatePaymentResponse
                {
                    RazorpayOrderId = payment.RazorpayOrderId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    KeyId = _config["Razorpay:KeyId"]!
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest request)
        {

            bool isValid = await _paymentService.VerifySignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

            if (!isValid)
                return BadRequest(new
                {
                    message = "Payment verification failed"
                });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var orderId = await _paymentService.MarkPaymentCapturedAsync(userId, request.Order, request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature);

                return Ok(new { message = "Payment verified successfully", orderId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);

            var payload = await reader.ReadToEndAsync();

            var signature = Request.Headers["X-Razorpay-Signature"].ToString();

            var webhookSecret = _config["Razorpay:WebhookSecret"];

            if (string.IsNullOrEmpty(webhookSecret))
                return StatusCode(500, "Webhook secret is not configured.");

            var isValid = _webhookService.VerifySignature(payload, signature, webhookSecret);

            if (!isValid)
                return Unauthorized();

            await _webhookService.ProcessWebhookAsync(payload);

            return Ok();
        }
    }
}