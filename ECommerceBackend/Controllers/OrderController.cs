using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Models;
using ECommerceBackend.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IApplicationLogger _logger;

        public OrderController(AppDbContext context, IApplicationLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            try
            {
                await _logger.LogMessageAsync("Order creation started - User is attempting to place an order.");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    await _logger.LogMessageAsync("Order creation failed - NameIdentifier claim was missing from the authenticated user.", "Warning");
                    return Unauthorized();
                }

                int userId = int.Parse(userIdClaim.Value);

                await _logger.LogMessageAsync($"COD order creation - UserId: {userId}");

                var result = await CreateOrderFromCart(userId, dto, "Cash on Delivery", "Pending");

                if (result.Order == null)
                {
                    await _logger.LogMessageAsync($"Order creation failed - Cart is empty for UserId: {userId}.", "Warning");

                    return BadRequest(new
                    {
                        message = "Your cart is empty."
                    });
                }

                await _logger.LogMessageAsync($"Order created successfully - OrderId: {result.Order.Id}, UserId: {userId}, ItemCount: {result.ItemCount}, TotalAmount: {result.Order.TotalAmount}, PaymentMethod: {result.Order.PaymentMethod}, PaymentStatus: {result.Order.PaymentStatus}");

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

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    await _logger.LogMessageAsync("Online payment failed - NameIdentifier claim was missing.", "Warning");
                    return Unauthorized();
                }

                int userId = int.Parse(userIdClaim.Value);

                await _logger.LogMessageAsync($"Online payment processing - UserId: {userId}");

                var result = await CreateOrderFromCart(userId, dto, "Online Payment", "Paid");

                if (result.Order == null)
                {
                    await _logger.LogMessageAsync($"Online payment failed - Cart is empty for UserId: {userId}.", "Warning");

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
                await _logger.LogMessageAsync($"Online payment failed - Unexpected error. Error: {ex.Message}, StackTrace: {ex.StackTrace}","Error");

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
                await _logger.LogMessageAsync("Admin requested all orders.");

                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        o.Id,
                        o.UserId,

                        CustomerName = o.User.Name,
                        CustomerEmail = o.User.Email,

                        o.DeliveryName,
                        o.Phone,
                        o.Address,
                        o.City,
                        o.State,
                        o.Pincode,

                        o.TotalAmount,
                        o.Status,
                        o.PaymentMethod,
                        o.PaymentStatus,
                        o.OrderDate,

                        OrderItems = o.OrderItems.Select(oi => new
                        {
                            oi.Id,
                            oi.ProductId,
                            ProductName = oi.Product.Name,
                            oi.Quantity,
                            oi.Price
                        })
                    })
                    .ToListAsync();

                await _logger.LogMessageAsync($"Admin retrieved all orders successfully - OrderCount: {orders.Count}");

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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    await _logger.LogMessageAsync("Get user orders failed - NameIdentifier claim was missing.", "Warning");

                    return Unauthorized();
                }

                int userId = int.Parse(userIdClaim.Value);

                await _logger.LogMessageAsync($"User requested their orders - UserId: {userId}");

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        o.Id,
                        o.DeliveryName,
                        o.Phone,
                        o.Address,
                        o.City,
                        o.State,
                        o.Pincode,
                        o.TotalAmount,
                        o.Status,
                        o.PaymentMethod,
                        o.PaymentStatus,
                        o.OrderDate,

                        OrderItems = o.OrderItems.Select(oi => new
                        {
                            oi.Id,
                            oi.ProductId,
                            ProductName = oi.Product.Name,
                            ProductImage = oi.Product.Images
                                .Select(i => i.ImageUrl)
                                .FirstOrDefault(),
                            oi.Quantity,
                            oi.Price
                        })
                    })
                    .ToListAsync();

                await _logger.LogMessageAsync($"User orders retrieved successfully - UserId: {userId}, OrderCount: {orders.Count}");

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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    await _logger.LogMessageAsync($"Order details request failed - NameIdentifier claim was missing. RequestedOrderId: {id}", "Warning");

                    return Unauthorized();
                }

                int userId = int.Parse(userIdClaim.Value);

                await _logger.LogMessageAsync($"User requested order details - UserId: {userId}, OrderId: {id}");

                var order = await _context.Orders
                    .Where(o => o.Id == id && o.UserId == userId)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Select(o => new
                    {
                        o.Id,
                        o.DeliveryName,
                        o.Phone,
                        o.Address,
                        o.City,
                        o.State,
                        o.Pincode,
                        o.TotalAmount,
                        o.Status,
                        o.PaymentMethod,
                        o.PaymentStatus,
                        o.OrderDate,

                        OrderItems = o.OrderItems.Select(oi => new
                        {
                            oi.Id,
                            oi.ProductId,
                            ProductName = oi.Product.Name,
                            ProductImage = oi.Product.Images
                                .Select(i => i.ImageUrl)
                                .FirstOrDefault(),
                            oi.Quantity,
                            oi.Price
                        })
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    await _logger.LogMessageAsync($"Order details not found - UserId: {userId}, RequestedOrderId: {id}. Order may not exist or may belong to another user.", "Warning");

                    return NotFound(new
                    {
                        message = "Order not found."
                    });
                }

                await _logger.LogMessageAsync($"Order details retrieved successfully - UserId: {userId}, OrderId: {id}, Status: {order.Status}, TotalAmount: {order.TotalAmount}");

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

        private async Task<(Order? Order, int ItemCount)> CreateOrderFromCart(int userId, CreateOrderDto dto, string paymentMethod, string paymentStatus)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return (null, 0);
            }

            decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);

            var order = new Order
            {
                UserId = userId,

                DeliveryName = dto.FullName,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Pincode = dto.Pincode,

                TotalAmount = totalAmount,
                Status = "Pending",

                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,

                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);// here order details are added to Order model

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                };

                _context.OrderItems.Add(orderItem);// here order productid, quantity, price are added to OrderItem model
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return (order, cartItems.Count);
        }
    }
}