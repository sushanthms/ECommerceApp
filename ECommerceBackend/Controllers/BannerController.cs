using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BannerController(AppDbContext context)
        {
            _context = context;
        }

        // Get all banners
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBanners()
        {
            var banners = await _context.Banners
                .ToListAsync();

            return Ok(banners);
        }

        // Get one banner
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return NotFound(new { message = "Banner not found." });
            }

            return Ok(banner);
        }

        // Add banner
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBanner(
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] string buttonText,
            [FromForm] string link,
            [FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(new { message = "Banner image is required." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only JPG, JPEG, PNG and WEBP images are allowed." });
            }

            var bannersFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "banners"
            );

            if (!Directory.Exists(bannersFolder))
            {
                Directory.CreateDirectory(bannersFolder);
            }

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

            return Ok(new
            {
                message = "Banner added successfully.",
                banner
            });
        }

        // Update banner
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner(
            int id,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] string buttonText,
            [FromForm] string link,
            [FromForm] IFormFile? image)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return NotFound(new { message = "Banner not found." });
            }

            banner.Title = title;
            banner.Description = description;
            banner.ButtonText = buttonText;
            banner.Link = link;

            if (image != null && image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Only JPG, JPEG, PNG and WEBP images are allowed." });
                }

                var bannersFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "banners"
                );

                if (!Directory.Exists(bannersFolder))
                {
                    Directory.CreateDirectory(bannersFolder);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(bannersFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                banner.ImageUrl = "/banners/" + fileName;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Banner updated successfully.",
                banner
            });
        }

        // Delete banner
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return NotFound(new { message = "Banner not found." });
            }

            _context.Banners.Remove(banner);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Banner deleted successfully."
            });
        }
    }
}