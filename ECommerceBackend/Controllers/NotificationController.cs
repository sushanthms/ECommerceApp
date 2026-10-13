using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize(Roles = "Admin")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;
        public NotificationController(NotificationService service)
        {
            _service = service;
        }

        // /api/notifications
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetLatestAsync());

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount() => Ok(new { count = await _service.GetUnreadCountAsync() });

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id) { await _service.MarkReadAsync(id); return Ok(); }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead() { await _service.MarkAllReadAsync(); return Ok(); }
    }
}