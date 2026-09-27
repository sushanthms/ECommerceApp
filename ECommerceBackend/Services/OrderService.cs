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

        public async Task<(Order? Order, int ItemCount, List<string> UnavailableItems)> CreateOrderAsync(int userId, CreateOrderDto dto, string paymentMethod, string paymentStatus)
        {
            var result = await CreateOrderFromCart(userId, dto, paymentMethod, paymentStatus);

            return result;
        }

        // admin sees their orders
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

        private async Task<(Order? Order, int ItemCount, List<string> UnavailableItems)> CreateOrderFromCart(int userId, CreateOrderDto dto, string paymentMethod, string paymentStatus)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return (null, 0, new List<string>());
            }

            var unavailableItems = cartItems
                .Where(c => c.Product.IsDeleted)
                .Select(c => c.Product.Name)
                .ToList();

            if (unavailableItems.Count > 0)
            {
                var productIds = string.Join(", ", cartItems.Where(c => c.Product.IsDeleted).Select(c => c.ProductId));
                await _logger.LogMessageAsync($"Order creation failed - cart has removed the products. UserId: {userId}, ProductIds: {productIds}", "Warning");
                return (null, 0, unavailableItems);
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

            return (order, cartItems.Count, new List<string>());
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            await _logger.LogMessageAsync($"Admin requested order status update - OrderId: {id}, NewStatus: {status}");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                await _logger.LogMessageAsync($"Order status update failed - Order not found. OrderId: {id}", "Warning");

                return false;
            }

            if (order.Status == "Cancelled")
            {
                await _logger.LogMessageAsync($"Order status update failed - Cancelled order cannot be changed. OrderId: {id}", "Warning");
                return false;
            }
            // if the incoming status is cancelled, then CancelOrderAndRestoreStockAsync function runs, othewise the status is changed as per the incoming status
            if (status == "Cancelled")
            {
                // when changes the status to cance, the product stock is restored
                var cancelled = await CancelOrderAndRestoreStockAsync(id, null);

                if (!cancelled)
                {
                    await _logger.LogMessageAsync($"Order status update failed - order cannot be cancelled in its current state. OrderId: {id}", "Warning");
                    return false;
                }

                await _logger.LogMessageAsync($"Order cancelled by admin and stock restored - OrderId: {id}");
                return true;
            }

            // if the incoming status value is not "cancelled", then the next code runs
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

            var rows = await _context.Orders
                .Where(o => o.Id == id && o.Status != "Cancelled")
                .ExecuteUpdateAsync(s => s// this method does not need save changes to database
                    .SetProperty(o => o.Status, status)// changes the order status
                    .SetProperty(o => o.PaymentStatus,// changes the payment status
                        o => status == "Delivered" && o.PaymentMethod == "Cash on Delivery"
                             ? "Paid"
                             : o.PaymentStatus));// for online payment the status will be paid. this setproperty keeps the status paid as it is

            if (rows == 0)
            {
                await _logger.LogMessageAsync($"Order status update failed - order was cancelled meanwhile. OrderId: {id}", "Warning");
                return false;
            }

            await _logger.LogMessageAsync($"Order status updated successfully - OrderId: {id}, Status: {status}");

            return true;
        }

        public async Task<bool> CancelOrderAsync(int userId, int id)
        {
            var cancelled = await CancelOrderAndRestoreStockAsync(id, userId);

            if (!cancelled)
            {
                await _logger.LogMessageAsync($"Order cancellation failed - not found or cannot be cancelled. UserId: {userId}, OrderId: {id}", "Warning");
                return false;
            }

            await _logger.LogMessageAsync($"Order cancelled successfully and stock restored - UserId: {userId}, OrderId: {id}");
            return true;
        }

        // CancelOrderAsync(user) calls this function with the user's id. admin calls this function with null, which means "any user's order".
        private async Task<bool> CancelOrderAndRestoreStockAsync(int orderId, int? userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var query = _context.Orders.Where(o =>
                    o.Id == orderId &&
                    (o.Status == "Pending" || o.Status == "Processing"));

                if (userId != null)
                {
                    query = query.Where(o => o.UserId == userId.Value);// userId can be null, so to get the id we write userId.Value
                }

                // ExecuteUpdateAsync sets the status to Cancelled
                var rows = await query.ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Cancelled"));
                // it gives output 0 means it cant be cancelled( means shipped or already cancelled)
                // prevents double clicking cancel

                if (rows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                // i.Order.Id is the Id of the parent order (from the Orders table) that a given OrderItem row belongs to.
                var items = await _context.OrderItems// OrderItems contains, OrderId, ProductId, Quantity, Price
                    .Where(i => i.Order.Id == orderId)// Order contains detail of the order. i.Order.Id means the id of the individual product order in the Order table
                    .Select(i => new { i.ProductId, i.Quantity })
                    .ToListAsync();

                foreach (var item in items)
                {
                    await _context.Products
                        .Where(p => p.Id == item.ProductId)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}