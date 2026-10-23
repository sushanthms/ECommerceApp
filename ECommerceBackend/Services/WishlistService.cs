using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services
{
    public class WishlistService
    {
        private readonly AppDbContext _context;

        public WishlistService(AppDbContext context)
        {
            _context = context;
        }

        // returns false if the product does not exist or hidden
        public async Task<bool> AddAsync(int userId, int productId)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId && !p.IsDeleted);

            if (!productExists)
            {
                return false;
            }

            var alreadySaved = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            if (alreadySaved)
            {
                return true;// adding twice is not an error
            }

            _context.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // two requests raced; the unique index rejected the second one.
                // the product is in the wishlist either way, so treats it as success.
            }

            return true;
        }

        public async Task<bool> RemoveAsync(int userId, int productId)
        {
            var rows = await _context.WishlistItems
                .Where(w => w.UserId == userId && w.ProductId == productId)
                .ExecuteDeleteAsync();

            return rows > 0;
        }

        public async Task<(List<object> Items, int Total, int Page, int PageSize)> GetAsync(int userId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 12;

            var query = _context.WishlistItems
                .AsNoTracking()
                .Where(w => w.UserId == userId && !w.Product.IsDeleted);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(w => w.CreatedAt)
                .ThenByDescending(w => w.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new
                {
                    w.ProductId,
                    w.Product.Name,
                    w.Product.Price,
                    w.Product.Stock,
                    w.Product.Category,
                    Images = w.Product.Images
                        .Select(i => new { i.ImageUrl })
                        .ToList(),
                    w.CreatedAt
                })
                .Cast<object>()
                .ToListAsync();

            return (items, total, page, pageSize);
        }

        public async Task<List<int>> GetIdsAsync(int userId)
        {
            return await _context.WishlistItems
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .Select(w => w.ProductId)
                .ToListAsync();
        }
    }
}