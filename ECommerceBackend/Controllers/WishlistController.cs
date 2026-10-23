using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class WishlistController : ControllerBase
    {
        private readonly WishlistService _wishlistService;

        public WishlistController(WishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var added = await _wishlistService.AddAsync(userId.Value, productId);

            if (!added)
            {
                return NotFound(new { message = "Product not found." });
            }

            return Ok(new { message = "Added to wishlist." });
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var removed = await _wishlistService.RemoveAsync(userId.Value, productId);

            if (!removed)
            {
                return NotFound(new { message = "Item not in wishlist." });
            }

            return Ok(new { message = "Removed from wishlist." });
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 12)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _wishlistService.GetAsync(userId.Value, page, pageSize);

            return Ok(new
            {
                items = result.Items,
                page = result.Page,
                pageSize = result.PageSize,
                totalItems = result.Total,
                hasMore = result.Page * result.PageSize < result.Total
            });
        }

        [HttpGet("ids")]
        public async Task<IActionResult> GetIds()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _wishlistService.GetIdsAsync(userId.Value));
        }
    }
}