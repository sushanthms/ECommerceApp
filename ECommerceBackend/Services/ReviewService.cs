using ECommerceBackend.Data;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services
{
    public class ReviewService
    {
        private readonly AppDbContext _context;
        private readonly IApplicationLogger _logger;

        public ReviewService(AppDbContext context, IApplicationLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        // Add review
        public async Task<(Review? Review, string? Error, string? UserName)> AddReviewAsync(Review review, int userId)
        {
            await _logger.LogMessageAsync($"Add review started - UserId: {userId}, ProductId: {review.ProductId}, Rating: {review.Rating}");

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == review.ProductId &&
                    !p.IsDeleted);

            if (product == null)
            {
                await _logger.LogMessageAsync($"Add review failed - Product not found or is deleted. UserId: {userId}, ProductId: {review.ProductId}","Warning");

                return (null, "Product not found.", null);
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                await _logger.LogMessageAsync($"Add review failed - Invalid rating. UserId: {userId}, ProductId: {review.ProductId}, Rating: {review.Rating}", "Warning");

                return (null, "Rating must be between 1 and 5.", null);
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.ProductId == review.ProductId);

            if (existingReview != null)
            {
                await _logger.LogMessageAsync($"Add review failed - User has already reviewed this product. UserId: {userId}, ProductId: {review.ProductId}", "Warning");

                return (null, "You have already reviewed this product.", null);
            }

            review.UserId = userId;
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            await _logger.LogMessageAsync($"Review added successfully - ReviewId: {review.Id}, UserId: {userId}, ProductId: {review.ProductId}, Rating: {review.Rating}");

            return (review, null, user?.Name);
        }

        // Get reviews for one product
        public async Task<List<object>> GetProductReviewsAsync(int productId)
        {
            await _logger.LogMessageAsync($"Product reviews requested - ProductId: {productId}");

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
                .Cast<object>()
                .ToListAsync();

            await _logger.LogMessageAsync($"Product reviews retrieved successfully - ProductId: {productId}, ReviewCount: {reviews.Count}");

            return reviews;
        }

        // Get all reviews for admin
        public async Task<List<object>> GetAllReviewsAsync()
        {
            await _logger.LogMessageAsync("Admin requested all reviews.");

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
                .Cast<object>()
                .ToListAsync();

            await _logger.LogMessageAsync($"Admin retrieved all reviews successfully - ReviewCount: {reviews.Count}");

            return reviews;
        }

        // Delete review
        public async Task<bool> DeleteReviewAsync(int id)
        {
            await _logger.LogMessageAsync($"Delete review started - ReviewId: {id}");

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                await _logger.LogMessageAsync($"Delete review failed - Review not found. ReviewId: {id}", "Warning");

                return false;
            }

            _context.Reviews.Remove(review);

            await _context.SaveChangesAsync();

            await _logger.LogMessageAsync($"Review deleted successfully - ReviewId: {id}");

            return true;
        }
    }
}