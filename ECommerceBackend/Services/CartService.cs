using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services
{
    public class CartService
    {
        private readonly AppDbContext _context;
        // we cannot reassign another AppDbContext to _context. only in this constructor we can assign AppDbContext to _context
        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? Message, int? Stock, bool NotFound)> AddToCartAsync(int userId, AddToCartDto dto)
        {// Task means the method performs an asynchronous operation and will eventually produce a result.
            // Inside the task we have return type as tuple, Tuple allows to return multiple in the return
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == dto.ProductId &&
                    !p.IsDeleted);

            if (product == null)
            {
                return (false, "Product not found.", null, true);
            }

            if (dto.Quantity <= 0)// quantity we put to cart
            {
                return (false, "Quantity must be at least 1.", null, false);
            }

            // A transaction groups multiple database operations together.
            // Starting a transaction. Stock change and cart change must happen together.
            // using is used to dispose the transaction connection inside the transaction variable otherwise we have to manually write dispose inside finally block
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ExecuteSqlInterpolatedAsync() returns the number of database rows affected by the SQL statement.
                // here output will be 0 or 1
                // $ allows interpolation, {dto.Quantity}
                // @ allows to write SQL commands in multiple lines without using \n
                var rowsAffected =
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        UPDATE Products
                        SET Stock = Stock - {dto.Quantity}
                        WHERE Id = {dto.ProductId}
                        AND Stock >= {dto.Quantity}
                    ");// // changes the stock based on the where condition

                // 0 means the database could not decrease the stock.
                if (rowsAffected == 0)
                {
                    var currentStock = await _context.Products
                        .Where(p => p.Id == dto.ProductId)
                        .Select(p => p.Stock)
                        .FirstAsync();

                    await transaction.RollbackAsync();

                    return (false, $"Only {currentStock} available.", currentStock, false);
                }

                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);

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

                await _context.SaveChangesAsync();

                // Reloading the product row because we changed its Stock using raw SQL above
                await _context.Entry(product).ReloadAsync();

                await transaction.CommitAsync();

                return (true, "Product added to cart.", product.Stock, false);
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        public async Task<List<CartItemDto>>GetCartAsync(int userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Select(c => new CartItemDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    Name = c.Product.Name,
                    Price = c.Product.Price,

                    ImageUrl = c.Product.Images
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    Stock = c.Product.Stock,
                    Quantity = c.Quantity
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Message, int? Stock, bool NotFound)> UpdateQuantityAsync(int userId, int id, UpdateCartItemDto dto)
        {
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.UserId == userId);

            if (cartItem == null)
            {
                return (false, null, null, true);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try {
                // If quantity is 0 or less, removing the item.
                if (dto.Quantity <= 0)
                {
                    cartItem.Product.Stock += cartItem.Quantity;

                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, null, cartItem.Product.Stock, false);
                }
                
                
                int quantityDifference = dto.Quantity - cartItem.Quantity;

            if (quantityDifference > 0)
            {
                var rowsAffected =
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        UPDATE Products
                        SET Stock = Stock - {quantityDifference}
                        WHERE Id = {cartItem.ProductId}
                        AND Stock >= {quantityDifference}
                    ");

                // The database could not reserve the additional stock.
                if (rowsAffected == 0)
                {
                    var currentStock = await _context.Products
                        .Where(p => p.Id == cartItem.ProductId)
                        .Select(p => p.Stock)
                        .FirstAsync();

                 await transaction.RollbackAsync();
                 return (false, $"Only {currentStock} more in stock.", currentStock, false);
                }
            }
            else if (quantityDifference < 0)
            {
                // The user decreased the quantity.
                int stockToReturn = -quantityDifference;

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE Products
                    SET Stock = Stock + {stockToReturn}
                    WHERE Id = {cartItem.ProductId}
                ");
            }
            
            cartItem.Quantity = dto.Quantity;

            await _context.SaveChangesAsync();

            await _context.Entry(cartItem.Product).ReloadAsync();
            await transaction.CommitAsync();
            return (true, null, cartItem.Product.Stock, false);
        }

            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(bool Success, int? Stock)> RemoveFromCartAsync(int userId, int id)
        {
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.UserId == userId);

            if (cartItem == null)
            {
                return (false, null);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE Products
                    SET Stock = Stock + {cartItem.Quantity}
                    WHERE Id = {cartItem.ProductId}
                ");

                _context.CartItems.Remove(cartItem);

                await _context.SaveChangesAsync();

                await _context.Entry(cartItem.Product).ReloadAsync();

                await transaction.CommitAsync();

                return (true, cartItem.Product.Stock);
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }
    }
}