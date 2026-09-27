using ECommerceBackend.DTOs;
using ECommerceBackend.Models;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> AddReview(AddReviewDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim);

            var result = await _reviewService.AddReviewAsync(dto, userId);

            if (result.Review == null)
            {
                if (result.Error == "Product not found.")
                {
                    return NotFound(new { message = result.Error });
                }

                return BadRequest(new { message = result.Error });
            }

            return Ok(new
            {
                result.Review.Id,
                result.Review.ProductId,
                result.Review.Rating,
                result.Review.Comment,
                result.Review.CreatedAt,
                UserName = result.UserName
            });
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;// to prevent a person to send page -1 or pagsize 1000 from the frontend

            var (reviews, totalCount, averageRating) = await _reviewService.GetProductReviewsAsync(productId, page, pageSize);

            return Ok(new { reviews, totalCount, averageRating, page, pageSize });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();

            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var success = await _reviewService.DeleteReviewAsync(id);

            if (!success)
            {
                return NotFound(new { message = "Review not found." });
            }

            return Ok(new { message = "Review deleted successfully." });
        }

        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOwnReview(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim);

            var (success, error) = await _reviewService.DeleteOwnReviewAsync(id, userId);

            if (!success)
            {
                if (error == "Review not found.")
                {
                    return NotFound(new { message = error });
                }

                return BadRequest();
            }

            return Ok(new { message = "Review deleted successfully." });
        }
    }
}