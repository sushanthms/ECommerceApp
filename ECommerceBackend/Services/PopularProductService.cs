using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StackExchange.Redis;

namespace ECommerceBackend.Services
{
    public class PopularProductsService
    {
        private const string PopularKey = "ecommerce:popular:products";
        private const int RecentLimit = 10;
        private static readonly TimeSpan RecentExpiry = TimeSpan.FromDays(30);
        private readonly IConnectionMultiplexer _redis;
        private readonly AppDbContext _context;

        public PopularProductsService(IConnectionMultiplexer redis, AppDbContext context)
        {
            _redis = redis;
            _context = context;
        }

        private static string RecentKey(int userId) => $"ecommerce:recent:{userId}";

        public async Task RecordViewAsync(int productId)
        {
            try
            {
                // under the popularkey name field all the productid and count row is stored as list
                await _redis.GetDatabase().SortedSetIncrementAsync(PopularKey, productId, 1);
            }
            catch
            {
                // counting is optional: if Redis is down, ignore it
                // it prevents a Redis failure from breaking the product-detail request because of the counting operation.
            }
        }

        public async Task<List<ProductListDto>> GetTopAsync(int count)
        {
            count = Math.Clamp(count, 1, 20);

            RedisValue[] members;// SortedSetRangeByRankAsync() returns an array of Redis values, we store it in array called as members
            try
            {
                // under the popularkey name field all the productid and count row is stored as list
                // count * 2-1 means if count is 10 we get (10*2)-1=20-1=19, we start from 0, so we get 20 products. redis includes the last index
                members = await _redis.GetDatabase().SortedSetRangeByRankAsync(PopularKey, 0, count * 2 - 1, Order.Descending);
            }
            catch
            {
                return new List<ProductListDto>();
            }

            // RedisValue[] members is an array of RedisValue elements returned by Redis, LoadInOrderAsync requires ids as integers
            var ids = members.Select(m => (int)m).ToList();
            return await LoadInOrderAsync(ids, count);
        }

        public async Task RecordRecentAsync(int userId, int productId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = RecentKey(userId);

                var batch = db.CreateBatch();// This allows to queue multiple Redis commands and submit them together using batch.Execute().
                _ = batch.ListRemoveAsync(key, productId);// if the user a product that was already in the recently viewed list, then it removes that product id from the recently viewed list and puts it to the front place of the recently viewed list
                _ = batch.ListLeftPushAsync(key, productId);
                _ = batch.ListTrimAsync(key, 0, RecentLimit - 1);
                _ = batch.KeyExpireAsync(key, RecentExpiry);
                batch.Execute();
                await Task.CompletedTask;
            }
            catch
            {

            }
        }

        public async Task<List<ProductListDto>> GetRecentAsync(int userId, int count)
        {
            count = Math.Clamp(count, 1, RecentLimit);

            RedisValue[] members;
            try
            {
                members = await _redis.GetDatabase().ListRangeAsync(RecentKey(userId), 0, count * 2 - 1);
            }
            catch
            {
                return new List<ProductListDto>();
            }

            var ids = members.Select(m => (int)m).ToList();
            return await LoadInOrderAsync(ids, count);
        }

        private async Task<List<ProductListDto>> LoadInOrderAsync(List<int> ids, int count)
        {
            if (ids.Count == 0) return new List<ProductListDto>();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    Category = p.Category,
                    Images = p.Images.Select(i => new ProductImageDto { ImageUrl = i.ImageUrl }).ToList()
                })
                .ToListAsync();

            // redis contains some data in the ids, but that product may be deleted after some time. so when a user calls LoadInOrderAsync(GetTopProducts) we check again whether the product is deleted or not.
            // the products table wont return all the products that are written in the ids because some products may be deleted

            // it is used to keep the Redis order while getting product details from SQL Server.
            // by[id] contains the product data
            var byId = products.ToDictionary(p => p.Id);// This converts the products list into a dictionary where the key is the product ID and the value is the complete product object.
            return ids.Where(byId.ContainsKey).Select(id => byId[id]).Take(count).ToList();
        }
    }
}