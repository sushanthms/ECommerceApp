using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Models;
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

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Your cart is empty."
                });
            }

            decimal totalAmount = cartItems.Sum(
                item => item.Product.Price * item.Quantity
            );

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
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            // Create multiple OrderItems for one order
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

            // Remove items from cart
            _context.CartItems.RemoveRange(cartItems);

            // Save everything
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order placed successfully.",
                orderId = order.Id,
                totalAmount = order.TotalAmount
            });
        }
        // Orders table has userid, userid is the foriegn key, using userid it accessess Users table and can get the details of the user. public User User { get; set; } this helps to navigate from the Order to User table
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
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

            return Ok(orders);
        }
    }
}