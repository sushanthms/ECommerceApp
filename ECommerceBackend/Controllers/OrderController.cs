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
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(value, out var userId) ? userId : null;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            // ControllerBase gives the controller access to HttpContext
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

                await _logger.LogMessageAsync($"Order creation - UserId: {userId}, PaymentMethod: Cash on Delivery, CorrelationId: {correlationId}");

                var result = await _orderService.CreateOrderAsync(userId.Value, dto, "Cash on Delivery", "Pending");

                if (result.UnavailableItems.Count > 0)
                {
                    return BadRequest(new
                    {
                        message = "Some items in your cart are no longer available. Please remove them and try again.",
                        unavailableItems = result.UnavailableItems
                    });
                }

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

            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
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

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrdersForAdmin(int page = 1, int pageSize = 20)
        {
            var result = await _orderService.GetOrdersPagedAsync(page, pageSize);

            return Ok(new
            {
                orders = result.Orders,
                page,
                pageSize,
                totalOrders = result.TotalOrders,
                totalPages = result.TotalPages
            });
        }

        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderStats()
        {
            return Ok(await _orderService.GetOrderStatsAsync());
        }

        [HttpGet("admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderForAdmin(int id)
        {
            var order = await _orderService.GetOrderDetailsForAdminAsync(id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return Ok(order);
        }

        [HttpGet("UserOrders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserOrders(int page = 1, int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                {
                    await _logger.LogMessageAsync("Get user orders failed - NameIdentifier claim was missing.", "Warning");

                    return Unauthorized();
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var result = await _orderService.GetUserOrdersAsync(userId.Value, page, pageSize);

                return Ok(new
                {
                    orders = result.Orders,
                    page,
                    pageSize,
                    totalOrders = result.TotalOrders,
                    hasMore = page * pageSize < result.TotalOrders
                });
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

        [HttpPut("User/{id}/address")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ChangeOrderAddress(int id, CreateOrderDto dto)
        {
            try
            {
                var userId = GetUserId();

                if (userId == null)
                    return Unauthorized();

                var success = await _orderService.ChangeOrderAddressAsync(userId.Value, id, dto);

                if (!success)
                    return BadRequest(new { message = "Address cannot be changed for this order." });

                return Ok(new { message = "Delivery address updated." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"Failed to change order address - OrderId: {id}, Error: {ex.Message}", "Error");
                return StatusCode(500, new { message = "An unexpected error occurred while updating the address." });
            }
        }

        [HttpGet("Addresses")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserAddresses()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var addresses = await _orderService.GetUserAddressesAsync(userId);

            return Ok(addresses);
        }

    }
}