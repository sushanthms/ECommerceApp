using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context)
        {// AppDbContext says: "whatever value gets passed in here must be an object of type AppDbContext."
         // It gets a reference (a memory address) pointing to the fully-built AppDbContext.
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)// dto is a variable/parameter that this function accepts, but it should be of the type AddToCartDto.
        {// ASP.NET Core's model binding checks whether the parameter is same type as it is required. model binding reports a validation/model-state error
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            // Claim means a piece of information about the user stored inside the authentication token.
            int userId = int.Parse(userIdClaim.Value);
            // we use the dto parameter to access the ProductId property of the AddToCartDto object it refers to.
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (dto.Quantity <= 0)
            {
                return BadRequest(new { message = "Quantity must be at least 1." });
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.ProductId == dto.ProductId);

            int currentQuantityInCart = existingCartItem?.Quantity ?? 0;// ternaray operation. true means existingCartItem.Quantity or false means 0

            if (currentQuantityInCart + dto.Quantity > product.Stock)
            {
                return BadRequest(new { message = $"Only {product.Stock - currentQuantityInCart} more available." });
            }

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += dto.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                _context.CartItems.Add(cartItem);
            }

            product.Stock -= dto.Quantity;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product added to cart.",
                stock = product.Stock
            });
        }

            [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            // A claim is a key value, value is string, id is also converted to string and stored in claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)// && is sued when we want a entity that meets a condtion, here we used Include means we want many entities.
                .Include(c => c.Product)// we wan   t all product entities in the cart of this user
                .Select(c => new
                {
                    c.Id,
                    c.ProductId,
                    c.Product.Name,
                    c.Product.Price,
                    c.Product.ImageUrl,
                    c.Product.Stock,
                    c.Quantity
                })
                .ToListAsync();

            return Ok(cartItems);
        }

        // PUT /api/Cart/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, UpdateCartItemDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (dto.Quantity <= 0)
            {
                cartItem.Product.Stock += cartItem.Quantity;

                _context.CartItems.Remove(cartItem);
            }
            else
            {
                int quantityDifference = dto.Quantity - cartItem.Quantity;

                if (quantityDifference > cartItem.Product.Stock)
                {
                    return BadRequest(new
                    {
                        message = $"Only {cartItem.Product.Stock} more in stock."
                    });
                }

                cartItem.Product.Stock -= quantityDifference;
                cartItem.Quantity = dto.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                stock = cartItem.Product.Stock
            });
        }

        // DELETE /api/Cart/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Product.Stock += cartItem.Quantity;

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                stock = cartItem.Product.Stock
            });
        }
    }
}