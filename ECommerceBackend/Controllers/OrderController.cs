using ECommerceBackend.DTOs;
using ECommerceBackend.Logging;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IApplicationLogger _logger;

        public OrderController(OrderService orderService, IApplicationLogger logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return null;
            }
            return int.Parse(userIdClaim.Value);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            try
            {
                await _logger.LogMessageAsync($"Order creation started - User is attempting to place an order. CorrelationId: {correlationId}");

                var userId = GetUserId();
                if (userId == null)
                {
                    await _logger.LogMessageAsync("Order creation failed - NameIdentifier claim was missing from the authenticated user.", "Warning");
                    return Unauthorized();
                }

                await _logger.LogMessageAsync($"COD order creation - UserId: {userId}, CorrelationId: {correlationId}");

                var result = await _orderService.CreateOrderAsync(userId.Value, dto, "Cash on Delivery","Pending");

                if (result.Order == null)
                {
                    await _logger.LogMessageAsync($"Order creation failed - Cart is empty for UserId: {userId}.", "Warning");

                    return BadRequest(new
                    {
                        message = "Your cart is empty."
                    });
                }

                await _logger.LogMessageAsync(
                    $"Order created successfully - OrderId: {result.Order.Id}, UserId: {userId}, " +
                    $"ItemCount: {result.ItemCount}, TotalAmount: {result.Order.TotalAmount}, " +
                    $"PaymentMethod: {result.Order.PaymentMethod}, PaymentStatus: {result.Order.PaymentStatus}, " +
                    $"CorrelationId: {correlationId}");

                return Ok(new
                {
                    message = "Order placed successfully.",
                    orderId = result.Order.Id,
                    totalAmount = result.Order.TotalAmount,
                    paymentMethod = result.Order.PaymentMethod,
                    paymentStatus = result.Order.PaymentStatus
                });
            }

            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Order creation failed - Unexpected error. Error: {ex.Message}, StackTrace: {ex.StackTrace}", "Error");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while placing the order."
                });
            }
        }

        [HttpPost("Pay")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ProcessPayment(CreateOrderDto dto)
        {
            try
            {
                await _logger.LogMessageAsync("Online payment started - User is attempting to make a payment.");

                var userId = GetUserId();
                if (userId == null)
                {
                    await _logger.LogMessageAsync("Online payment failed - NameIdentifier claim was missing.", "Warning");
                    return Unauthorized();
                }

                await _logger.LogMessageAsync($"Online payment processing - UserId: {userId}");

                var result = await _orderService.CreateOrderAsync(userId.Value, dto, "Online Payment", "Paid");

                if (result.Order == null)
                {
                    await _logger.LogMessageAsync($"Online payment failed - Cart is empty for UserId: {userId.Value}.", "Warning");

                    return BadRequest(new
                    {
                        message = "Your cart is empty."
                    });
                }

                await _logger.LogMessageAsync($"Online payment successful - OrderId: {result.Order.Id}, UserId: {userId}, TotalAmount: {result.Order.TotalAmount}, PaymentMethod: {result.Order.PaymentMethod}, PaymentStatus: {result.Order.PaymentStatus}");

                return Ok(new
                {
                    message = "Payment successful and order placed.",
                    orderId = result.Order.Id,
                    totalAmount = result.Order.TotalAmount,
                    paymentMethod = result.Order.PaymentMethod,
                    paymentStatus = result.Order.PaymentStatus
                });
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Online payment failed - Unexpected error. Error: {ex.Message}, StackTrace: {ex.StackTrace}", "Error");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while processing the payment."
                });
            }
        }

        // Orders table has userid, userid is the foriegn key, using userid it accessess Users table and can get the details of the user. public User User { get; set; } this helps to navigate from the Order to User table
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Failed to retrieve all orders - Unexpected database or server error. Error: {ex.Message}, StackTrace: {ex.StackTrace}", "Error");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while retrieving orders."
                });
            }
        }

        [HttpGet("UserOrders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserOrders()
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                {
                    await _logger.LogMessageAsync("Get user orders failed - NameIdentifier claim was missing.", "Warning");

                    return Unauthorized();
                }

                var orders = await _orderService.GetUserOrdersAsync(userId.Value);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Failed to retrieve user orders - Unexpected error. Error: {ex.Message}, StackTrace: {ex.StackTrace}", "Error");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while retrieving your orders."
                });
            }
        }

        [HttpGet("User/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserOrderDetails(int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                {
                    await _logger.LogMessageAsync($"Order details request failed - NameIdentifier claim was missing. RequestedOrderId: {id}", "Warning");

                    return Unauthorized();
                }

                var order = await _orderService.GetUserOrderDetailsAsync(userId.Value, id);

                if (order == null)
                {
                    await _logger.LogMessageAsync($"Order details not found - UserId: {userId}, RequestedOrderId: {id}. Order may not exist or may belong to another user.", "Warning");

                    return NotFound(new
                    {
                        message = "Order not found."
                    });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Failed to retrieve order details - OrderId: {id}, Error: {ex.Message}, StackTrace: {ex.StackTrace}", "Error");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while retrieving the order."
                });
            }
        }

// Admin updates order status
[HttpPut("admin/{id}/status")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(id, status);

                if (!success)
                {
                    return BadRequest(new
                    {
                        message = "Invalid order ID or status."
                    });
                }

                return Ok(new
                {
                    message = "Order status updated successfully."
                });
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Failed to update order status - OrderId: {id}, Error: {ex.Message}, StackTrace: {ex.StackTrace}", "Error");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while updating the order status."
                });
            }
        }

        // User cancels their own order
        [HttpPut("User/{id}/cancel")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                {
                    await _logger.LogMessageAsync($"Order cancellation failed - NameIdentifier claim was missing. OrderId: {id}", "Warning");

                    return Unauthorized();
                }

                var success = await _orderService.CancelOrderAsync(userId.Value, id);

                if (!success)
                {
                    return BadRequest(new
                    {
                        message = "Order cannot be cancelled."
                    });
                }

                return Ok(new
                {
                    message = "Order cancelled successfully."
                });
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Failed to cancel order - OrderId: {id}, Error: {ex.Message}, StackTrace: {ex.StackTrace}", "Error");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while cancelling the order."
                });
            }
        }

    }
}