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
        public async Task<IActionResult> AddBanner(Banner banner)
        {
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
        public async Task<IActionResult> UpdateBanner(int id, Banner updatedBanner)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
            {
                return NotFound(new { message = "Banner not found." });
            }

            banner.Title = updatedBanner.Title;
            banner.Description = updatedBanner.Description;
            banner.ButtonText = updatedBanner.ButtonText;
            banner.ImageUrl = updatedBanner.ImageUrl;
            banner.Link = updatedBanner.Link;

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