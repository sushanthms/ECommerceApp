using ECommerceBackend.Models.DTOs;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;

        public ProfileController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var profile = await _profileService.GetProfileAsync(userId);

            if (profile == null)
                return NotFound();

            return Ok(profile);
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var addresses = await _profileService.GetAddressesAsync(userId);

            return Ok(addresses);
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress(AddressDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var address = await _profileService.AddAddressAsync(userId, dto);

            return Ok(address);
        }

        [HttpPut("addresses/{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, AddressDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var updated = await _profileService.UpdateAddressAsync(userId, addressId, dto);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("addresses/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var deleted = await _profileService.DeleteAddressAsync(userId, addressId);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPut("addresses/{addressId}/default")]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var updated = await _profileService.SetDefaultAddressAsync(userId, addressId);

            if (!updated)
                return NotFound();

            return NoContent();
        }
    }
}