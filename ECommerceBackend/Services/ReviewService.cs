using ECommerceBackend.DTOs;
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

        // (Review? Review, string? Error, string? UserName are the return types. AddReviewDto dto, int userId are the parameter/arguments
        public async Task<(Review? Review, string? Error, string? UserName)> AddReviewAsync(AddReviewDto dto, int userId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && !p.IsDeleted);

            if (product == null)
            {
                await _logger.LogMessageAsync($"Add review failed - Product not found or is deleted. UserId: {userId}, ProductId: {dto.ProductId}", "Warning");
                return (null, "Product not found.", null);
            }

            var hasBought = await _context.OrderItems.AnyAsync(oi =>
                  oi.ProductId == dto.ProductId &&
                  oi.Order.UserId == userId &&
                  oi.Order.Status != "Cancelled");

            if (!hasBought)
            {
                return (null, "You can review only products you have ordered.", null);
            }

            if (dto.Rating < 1 || dto.Rating > 5)
            {
                await _logger.LogMessageAsync($"Add review failed - Invalid rating. UserId: {userId}, ProductId: {dto.ProductId}, Rating: {dto.Rating}", "Warning");
                return (null, "Rating must be between 1 and 5.", null);
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == dto.ProductId);

            if (alreadyReviewed)
            {
                await _logger.LogMessageAsync($"Add review failed - User has already reviewed this product. UserId: {userId}, ProductId: {dto.ProductId}", "Warning");
                return (null, "You have already reviewed this product.", null);
            }

            // We build the Review ourselves, so the caller cannot control any other field
            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two requests at the same moment
                await _logger.LogMessageAsync($"Add review failed - duplicate review (race). UserId: {userId}, ProductId: {dto.ProductId}", "Warning");
                return (null, "You have already reviewed this product.", null);
            }

            var userName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();

            await _logger.LogMessageAsync($"Review added successfully - ReviewId: {review.Id}, UserId: {userId}, ProductId: {dto.ProductId}, Rating: {dto.Rating}");

            return (review, null, userName);
        }

        // Get reviews of one product for users
        public async Task<(List<ReviewDto> Reviews, int TotalCount, double AverageRating)> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10)
        {
            await _logger.LogMessageAsync($"Product reviews requested - ProductId: {productId}, Page: {page}");

            var baseQuery = _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId);

            var totalCount = await baseQuery.CountAsync();

            // Computed over ALL reviews for this product, not just the current page
            var averageRating = totalCount > 0
                ? await baseQuery.AverageAsync(r => r.Rating)
                : 0;

            var reviews = await baseQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _context.Users,
                    review => review.UserId,
                    user => user.Id,
                    (review, user) => new ReviewDto
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        UserId = review.UserId,
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreatedAt = review.CreatedAt,
                        UserName = user.Name
                    })
                .ToListAsync();

            await _logger.LogMessageAsync($"Product reviews retrieved - ProductId: {productId}, Returned: {reviews.Count}, Total: {totalCount}");

            return (reviews, totalCount, averageRating);
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

        public async Task<(bool Success, string? Error)> DeleteOwnReviewAsync(int id, int userId)
        {
            await _logger.LogMessageAsync($"Delete own review started - ReviewId: {id}, UserId: {userId}");

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                await _logger.LogMessageAsync($"Delete own review failed - Review not found. ReviewId: {id}", "Warning");
                return (false, "Review not found.");
            }

            if (review.UserId != userId)
            {
                await _logger.LogMessageAsync($"Delete own review blocked - not owner. ReviewId: {id}, RequestingUserId: {userId}, OwnerUserId: {review.UserId}", "Warning");
                return (false, "You can only delete your own reviews.");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            await _logger.LogMessageAsync($"Own review deleted successfully - ReviewId: {id}, UserId: {userId}");
            return (true, null);
        }
    }
}