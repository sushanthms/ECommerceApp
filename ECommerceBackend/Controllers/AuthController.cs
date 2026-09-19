using ECommerceBackend.DTOs;
using ECommerceBackend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;// ASP.NET Core's web framework tools. ControllerBase, [ApiController], IActionResult

namespace ECommerceBackend.Controllers
{
    [ApiController]// automatically gives response like 400 based on the data input.
    [Route("api/[controller]")]
    public class AuthController : ControllerBase // things like Ok(), BadRequest()
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Registration of User
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)// IActionResult knows how to produce/send the HTTP response. describes what should be sent
        {
            try
            {
                bool registered = await _authService.RegisterAsync(request);

                if (!registered)
                {
                    return BadRequest(new
                    {
                        message = "Email is already registered."
                    });
                }

                return Ok(new
                {
                    message = "Registration successful."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }

        // Login of User and Admin
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            return Ok(new
            {
                message = "Login successful.",
                token = result.Token,
                userId = result.User!.Id,
                name = result.User.Name,
                email = result.User.Email,
                role = result.User.Role
            });
        }

[Authorize]
[HttpGet("verify")]
public IActionResult VerifyToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                userId,
                role
            });
        }

    }
}