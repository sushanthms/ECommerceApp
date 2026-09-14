using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> AddReview(Review review)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == review.ProductId && !p.IsDeleted);

            if (product == null)
                return NotFound("Product not found.");

            if (review.Rating < 1 || review.Rating > 5)
                return BadRequest("Rating must be between 1 and 5.");

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.ProductId == review.ProductId);

            if (existingReview != null)
                return BadRequest("You have already reviewed this product.");

            review.UserId = userId;
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return Ok(new
            {
                review.Id,
                review.ProductId,
                review.Rating,
                review.Comment,
                review.CreatedAt,
                UserName = user.Name
            });
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _context.Reviews// first table
                .Where(r => r.ProductId == productId)
                .Join(
                    _context.Users,// second table to join with
                    review => review.UserId,// from a review, use its UserId. LEFT side of the condition
                    user => user.Id,// from a user, use its Id. RIGHT side of the condition
                    (review, user) => new// after review.userid and user.id are matched, then it builds a new object by combining the data from both table
                    {
                        review.Id,
                        review.ProductId,
                        review.Rating,
                        review.Comment,
                        review.CreatedAt,
                        UserName = user.Name
                    })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .Join(
                    _context.Users,
                    review => review.UserId,
                    user => user.Id,
                    (review, user) => new
                    {
                        review,
                        user
                    })
                .Join(// The second Join() is performed on the result produced by the first Join()
                    _context.Products,
                    x => x.review.ProductId,// x represents the result from the first Join.
                    product => product.Id,
                    (x, product) => new
                    {
                        x.review.Id,
                        x.review.ProductId,
                        ProductName = product.Name,
                        UserId = x.user.Id,
                        UserName = x.user.Name,
                        x.review.Rating,
                        x.review.Comment,
                        x.review.CreatedAt
                    })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reviews);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound("Review not found.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Review deleted successfully.");
        }
    }
}