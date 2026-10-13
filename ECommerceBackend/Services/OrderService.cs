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

        public async Task<(List<object> Orders, int TotalOrders, int TotalPages)> GetOrdersPagedAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Orders.AsNoTracking();

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.DeliveryName,
                    o.OrderDate,
                    o.TotalAmount,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.Status
                })
                .Cast<object>()
                .ToListAsync();

            return (orders, totalOrders, totalPages);
        }

        public async Task<object?> GetOrderDetailsForAdminAsync(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
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
                .FirstOrDefaultAsync();

            return order;
        }

        public async Task<object> GetOrderStatsAsync()
        {
            var orders = _context.Orders.AsNoTracking();

            var totalOrders = await orders.CountAsync();
            var pendingOrders = await orders.CountAsync(o => o.Status == "Pending");

            var totalSales = await orders
                .Where(o => o.PaymentStatus == "Paid")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            var recentOrders = await orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    o.DeliveryName,
                    o.TotalAmount,
                    o.PaymentStatus,
                    o.Status
                })
                .ToListAsync();

            return new { totalOrders, pendingOrders, totalSales, recentOrders };
        }

        public async Task<List<object>> GetUserOrdersAsync(int userId)
        {
            await _logger.LogMessageAsync($"User requested their orders - UserId: {userId}");

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
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
            var address = await GetAddressAsync(userId, dto);

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

                DeliveryName = address.FullName,
                Phone = address.Phone,
                Address = string.Join(", ", new[] { address.Line1, address.Line2 }
                            .Where(s => !string.IsNullOrWhiteSpace(s))),
                City = address.City,
                State = address.State,
                Pincode = address.Pincode,

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

            // the user typed a new address and said "yes, save it"
            if (dto.AddressId == null && dto.SaveAddress)
            {
                try
                {
                    bool isFirst = !await _context.Addresses.AnyAsync(a => a.UserId == userId);

                    _context.Addresses.Add(new Address
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

        public async Task<bool> ChangeOrderAddressAsync(int userId, int orderId, CreateOrderDto dto)
        {
            var address = await GetAddressAsync(userId, dto);

            var fullAddress = string.Join(", ", new[] { address.Line1, address.Line2 }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            var rows = await _context.Orders
                .Where(o => o.Id == orderId
                         && o.UserId == userId
                         && (o.Status == "Pending" || o.Status == "Processing"))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.DeliveryName, address.FullName)
                    .SetProperty(o => o.Phone, address.Phone)
                    .SetProperty(o => o.Address, fullAddress)
                    .SetProperty(o => o.City, address.City)
                    .SetProperty(o => o.State, address.State)
                    .SetProperty(o => o.Pincode, address.Pincode));

            if (rows == 0)
            {
                await _logger.LogMessageAsync($"Order address change failed - UserId: {userId}, OrderId: {orderId}", "Warning");
                return false;
            }

            await _logger.LogMessageAsync($"Order address changed - UserId: {userId}, OrderId: {orderId}");
            return true;
        }

        public async Task<List<Address>> GetUserAddressesAsync(int userId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)// puts the default address first
                .ThenByDescending(a => a.Id)// sorts the remaining addresses by newest ID first.
                .ToListAsync();
        }

        public async Task<Address> GetAddressAsync(int userId, CreateOrderDto dto)
        {
            // case 1: the user picked a saved address
            if (dto.AddressId != null)
            {
                var saved = await _context.Addresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == dto.AddressId && a.UserId == userId);

                if (saved == null)
                    throw new ArgumentException("Please select a valid delivery address.");

                return saved;
            }

            // case 2: the user typed a new address
            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Phone) ||
                string.IsNullOrWhiteSpace(dto.Address) ||
                string.IsNullOrWhiteSpace(dto.City) ||
                string.IsNullOrWhiteSpace(dto.State) ||
                string.IsNullOrWhiteSpace(dto.Pincode))
                throw new ArgumentException("Please provide all delivery address details.");

            return new Address
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Line1 = dto.Address,
                City = dto.City,
                State = dto.State,
                Pincode = dto.Pincode
            };
        }
    }
}