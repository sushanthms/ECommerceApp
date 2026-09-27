using ECommerceBackend.DTOs;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly BannerService _bannerService;
        private const long MaxImageSize = 5 * 1024 * 1024;
        private static bool IsSafeLink(string link) =>
            link.StartsWith("/") ||
            (Uri.TryCreate(link, UriKind.Absolute, out var uri) &&
             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));

        public BannerController(BannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBanners()
        {
            var banners = await _bannerService.GetBannersAsync();

            return Ok(banners);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBanner(int id)
        {
            var banner = await _bannerService.GetBannerAsync(id);

            if (banner == null)
            {
                return NotFound(new { message = "Banner not found." });
            }

            return Ok(banner);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBanner([FromForm] BannerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Link))
            {
                return BadRequest(new { message = "Title and link are required." });
            }

            if (!IsSafeLink(dto.Link))
            {
                return BadRequest(new { message = "Link must start with / or http(s)://." });
            }

            if (dto.Image == null || dto.Image.Length == 0)
            {
                return BadRequest(new { message = "Banner image is required." });
            }

            if (dto.Image.Length > MaxImageSize)
            {
                return BadRequest(new { message = "Image must be 5 MB or smaller." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(dto.Image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only JPG, JPEG, PNG and WEBP images are allowed." });
            }

            var banner = await _bannerService.AddBannerAsync(
                dto.Title,
                dto.Description,
                dto.ButtonText,
                dto.Link,
                dto.Image
            );

            return CreatedAtAction(nameof(GetBanner), new { id = banner.Id }, new
            {
                message = "Banner added successfully.",
                banner
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] BannerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Link))
            {
                return BadRequest(new { message = "Title and link are required." });
            }

            if (!IsSafeLink(dto.Link))
            {
                return BadRequest(new { message = "Link must start with / or http(s)://." });
            }

            if (dto.Image != null && dto.Image.Length > 0)
            {
                if (dto.Image.Length > MaxImageSize)
                {
                    return BadRequest(new { message = "Image must be 5 MB or smaller." });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(dto.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Only JPG, JPEG, PNG and WEBP images are allowed." });
                }
            }

            var banner = await _bannerService.UpdateBannerAsync(
                id,
                dto.Title,
                dto.Description,
                dto.ButtonText,
                dto.Link,
                dto.Image
            );

            if (banner == null)
            {
                return NotFound(new { message = "Banner not found." });
            }

            return Ok(new
            {
                message = "Banner updated successfully.",
                banner
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var deleted = await _bannerService.DeleteBannerAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Banner not found." });
            }

            return Ok(new
            {
                message = "Banner deleted successfully."
            });
        }
    }
}