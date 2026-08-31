using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok(new
            {
                message = "This is a public API."
            });
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok(new
            {
                message = "You are authenticated!",
                user = User.Identity?.Name
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult Admin()
        {
            return Ok(new
            {
                message = "Welcome Admin!",
                user = User.Identity?.Name
            });
        }
    }
}