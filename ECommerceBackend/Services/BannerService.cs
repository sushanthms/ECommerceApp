using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerceBackend.Services
{
    public class BannerService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string BannersCacheKey = "banners";

        public BannerService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Banner>> GetBannersAsync()
        {
            if (_cache.TryGetValue(BannersCacheKey, out List<Banner>? cached))
            {
                return cached!;
            }

            var banners = await _context.Banners
                .AsNoTracking()
                .ToListAsync();

            _cache.Set(BannersCacheKey, banners, TimeSpan.FromMinutes(10));

            return banners;
        }
        public async Task<Banner?> GetBannerAsync(int id)
        {
            var banner = await _context.Banners
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            return banner;
        }

        public async Task<Banner> AddBannerAsync(string title, string description, string buttonText, string link, IFormFile image)
        {
            var bannersFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "banners");

            Directory.CreateDirectory(bannersFolder);

            var extension = Path.GetExtension(image.FileName).ToLower();
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(bannersFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var banner = new Banner
            {
                Title = title,
                Description = description,
                ButtonText = buttonText,
                Link = link,
                ImageUrl = "/banners/" + fileName
            };

            _context.Banners.Add(banner);

            await _context.SaveChangesAsync();
            _cache.Remove(BannersCacheKey);

            return banner;
        }

        public async Task<Banner?> UpdateBannerAsync( int id, string title, string description, string buttonText, string link, IFormFile? image)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return null;
            }

            banner.Title = title;
            banner.Description = description;
            banner.ButtonText = buttonText;
            banner.Link = link;

            if (image != null && image.Length > 0)
            {
                var bannersFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "banners");
                Directory.CreateDirectory(bannersFolder);

                var extension = Path.GetExtension(image.FileName).ToLower();
                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(bannersFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", banner.ImageUrl.TrimStart('/'));
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }

                banner.ImageUrl = "/banners/" + fileName;
            }

            await _context.SaveChangesAsync();
            _cache.Remove(BannersCacheKey);

            return banner;
        }

        // Delete banner
        public async Task<bool> DeleteBannerAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return false;
            }

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", banner.ImageUrl.TrimStart('/'));

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            _cache.Remove(BannersCacheKey);

            if (File.Exists(oldPath)) File.Delete(oldPath);

            return true;
        }
    }
}