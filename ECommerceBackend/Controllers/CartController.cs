using ECommerceBackend.DTOs;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{

    // app runs
    // React sends a request to the CartController. But ASP.NET needs to create a CartController object to execute the endpoint.
    // DI looks at the constructor and knows it needs CartService
    // It checks builder.Services.AddScoped<CartService>();
    // So DI creates/provides a CartService object.
    // now _cartService gets the CartService object through the cartService object.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {// CartService says: "whatever value gets passed in here must be an object of type CartService."
         // It gets a reference (a memory address) pointing to the fully-built CartService.
         // cartService is a parameter used to receive the CartService object that DI gives to the controller.
            _cartService = cartService;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)// Claim means a piece of information about the user stored inside the authentication token.
            {
                return null;
            }
            return int.Parse(userIdClaim.Value);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)// dto is a variable/parameter that this function accepts, but it should be of the type AddToCartDto.
        {// ASP.NET Core's model binding checks whether the parameter is same type as it is required. model binding reports a validation/model-state error
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _cartService.AddToCartAsync(userId.Value, dto);

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
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var cartItems = await _cartService.GetCartAsync(userId.Value);

            return Ok(cartItems);
        }

        // PUT /api/Cart/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, UpdateCartItemDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _cartService.UpdateQuantityAsync(userId.Value, id, dto);

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _cartService.RemoveFromCartAsync(userId.Value, id);

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
