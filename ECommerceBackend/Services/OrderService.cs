using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;
        private readonly IApplicationLogger _logger;

        public OrderService(AppDbContext context, IApplicationLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(Order? Order, int ItemCount)> CreateOrderAsync(
            int userId,
            CreateOrderDto dto,
            string paymentMethod,
            string paymentStatus)
        {
            var result = await CreateOrderFromCart(
                userId,
                dto,
                paymentMethod,
                paymentStatus
            );

            return result;
        }

        // Orders table has userid, userid is the foriegn key, using userid it accessess Users table and can get the details of the user. public User User { get; set; } this helps to navigate from the Order to User table
        public async Task<List<object>> GetAllOrdersAsync()
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
                .Cast<object>()
                .ToListAsync();

            await _logger.LogMessageAsync($"Admin retrieved all orders successfully - OrderCount: {orders.Count}");

            return orders;
        }

        public async Task<List<object>> GetUserOrdersAsync(int userId)
        {
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
                .Cast<object>()
                .ToListAsync();

            await _logger.LogMessageAsync($"User orders retrieved successfully - UserId: {userId}, OrderCount: {orders.Count}");

            return orders;
        }

        public async Task<object?> GetUserOrderDetailsAsync(int userId, int id)
        {
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

            return order;
        }

        private async Task<(Order? Order, int ItemCount)> CreateOrderFromCart(
            int userId,
            CreateOrderDto dto,
            string paymentMethod,
            string paymentStatus)
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

// Admin changes order status
public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            await _logger.LogMessageAsync($"Admin requested order status update - OrderId: {id}, NewStatus: {status}");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                await _logger.LogMessageAsync($"Order status update failed - Order not found. OrderId: {id}","Warning");

                return false;
            }

            if (order.Status == "Cancelled") 
            { await _logger.LogMessageAsync($"Order status update failed - Cancelled order cannot be changed. OrderId: {id}", "Warning"); 
                return false;
            }

            var allowedStatuses = new[]
            {
        "Pending",
        "Processing",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

            if (!allowedStatuses.Contains(status))
            {
                await _logger.LogMessageAsync($"Order status update failed - Invalid status. OrderId: {id}, Status: {status}", "Warning");

                return false;
            }

            order.Status = status;

            await _context.SaveChangesAsync();

            await _logger.LogMessageAsync($"Order status updated successfully - OrderId: {id}, Status: {status}");

            return true;
        }

// User cancels their own order
public async Task<bool> CancelOrderAsync(int userId, int id)
        {
            await _logger.LogMessageAsync($"User requested order cancellation - UserId: {userId}, OrderId: {id}");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.UserId == userId);

            if (order == null)
            {
                await _logger.LogMessageAsync($"Order cancellation failed - Order not found. UserId: {userId}, OrderId: {id}", "Warning");

                return false;
            }

            if (order.Status != "Pending" && order.Status != "Processing")
            {
                await _logger.LogMessageAsync($"Order cancellation failed - Order cannot be cancelled. UserId: {userId}, OrderId: {id}, Status: {order.Status}", "Warning");

                return false;
            }

            foreach (var orderItem in order.OrderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == orderItem.ProductId);

                if (product != null)
                {
                    product.Stock += orderItem.Quantity;
                }
            }

            order.Status = "Cancelled";

            await _context.SaveChangesAsync();

            await _logger.LogMessageAsync($"Order cancelled successfully and stock restored - UserId: {userId}, OrderId: {id}");

            return true;
        }

    }
}