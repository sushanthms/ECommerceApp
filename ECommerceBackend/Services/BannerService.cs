using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Services;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace ECommerceBackend.Services
{
    public class BannerService
    {
        private readonly AppDbContext _context;

        public BannerService(AppDbContext context)
        {
            _context = context;
        }

        // Get all banners
        public async Task<List<Banner>> GetBannersAsync()
        {
            var banners = await _context.Banners
                .ToListAsync();

            return banners;
        }

        // Get one banner
        public async Task<Banner?> GetBannerAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);

            return banner;
        }

        // Add banner
        public async Task<Banner> AddBannerAsync(
            string title,
            string description,
            string buttonText,
            string link,
            IFormFile image)
        {
            var bannersFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "banners");

            if (!Directory.Exists(bannersFolder))
            {
                Directory.CreateDirectory(bannersFolder);
            }

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

            return banner;
        }

        // Update banner
        public async Task<Banner?> UpdateBannerAsync(
            int id,
            string title,
            string description,
            string buttonText,
            string link,
            IFormFile? image)
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

                if (!Directory.Exists(bannersFolder))
                {
                    Directory.CreateDirectory(bannersFolder);
                }

                var extension = Path.GetExtension(image.FileName).ToLower();
                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(bannersFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                banner.ImageUrl = "/banners/" + fileName;
            }

            await _context.SaveChangesAsync();

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

            _context.Banners.Remove(banner);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}