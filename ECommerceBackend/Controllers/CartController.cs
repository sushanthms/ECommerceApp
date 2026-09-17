using ECommerceBackend.DTOs;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {// CartService says: "whatever value gets passed in here must be an object of type CartService."
         // It gets a reference (a memory address) pointing to the fully-built CartService.
            _cartService = cartService;
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

            var result = await _cartService.AddToCartAsync(userId, dto);

            if (result.NotFound)
            {
                return NotFound(result.Message);
            }

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                stock = result.Stock
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

            var cartItems = await _cartService.GetCartAsync(userId);

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

            var result = await _cartService.UpdateQuantityAsync(userId, id, dto);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(new
            {
                stock = result.Stock
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

            var result = await _cartService.RemoveFromCartAsync(userId, id);

            if (!result.Success)
            {
                return NotFound();
            }

            return Ok(new
            {
                stock = result.Stock
            });
        }
    }
}
