using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Models;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace ECommerceBackend.Services
{
    public class CartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? Message, int? Stock, bool NotFound)> AddToCartAsync(int userId, AddToCartDto dto)
        {
            // we use the dto parameter to access the ProductId property of the AddToCartDto object it refers to.
            var product = await _context.Products.FindAsync(dto.ProductId);

            if (product == null)
            {
                return (false, "Product not found.", null, true);
            }

            if (dto.Quantity <= 0)
            {
                return (false, "Quantity must be at least 1.", null, false);
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.ProductId == dto.ProductId);

            int currentQuantityInCart = existingCartItem?.Quantity ?? 0;// ternaray operation. true means existingCartItem.Quantity or false means 0

            if (currentQuantityInCart + dto.Quantity > product.Stock)
            {
                return (false, $"Only {product.Stock - currentQuantityInCart} more available.", null, false);
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

            return (true, "Product added to cart.", product.Stock, false);
        }

        public async Task<List<object>> GetCartAsync(int userId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)// && is sued when we want a entity that meets a condtion, here we used Include means we want many entities.
                .Include(c => c.Product)// we want all product entities in the cart of this user
                .ThenInclude(p => p.Images)
                .Select(c => new
                {
                    c.Id,
                    c.ProductId,
                    c.Product.Name,
                    c.Product.Price,
                    ImageUrl = c.Product.Images
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    c.Product.Stock,
                    c.Quantity
                })
                .Cast<object>()
                .ToListAsync();

            return cartItems;
        }

        public async Task<(bool Success, string? Message, int? Stock, bool NotFound)> UpdateQuantityAsync(
            int userId,
            int id,
            UpdateCartItemDto dto)
        {
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem == null)
            {
                return (false, null, null, true);
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
                    return (false, $"Only {cartItem.Product.Stock} more in stock.", null, false);
                }

                cartItem.Product.Stock -= quantityDifference;
                cartItem.Quantity = dto.Quantity;
            }

            await _context.SaveChangesAsync();

            return (true, null, cartItem.Product.Stock, false);
        }

        public async Task<(bool Success, int? Stock)> RemoveFromCartAsync(int userId, int id)
        {
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem == null)
            {
                return (false, null);
            }

            cartItem.Product.Stock += cartItem.Quantity;

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            return (true, cartItem.Product.Stock);
        }
    }
}