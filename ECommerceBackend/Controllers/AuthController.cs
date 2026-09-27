using ECommerceBackend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;// ASP.NET Core's web framework tools. ControllerBase, [ApiController], IActionResult

namespace ECommerceBackend.Controllers
{
    [ApiController]// http responses, mmodel binding and validation
    [Route("api/[controller]")]
    public class AuthController : ControllerBase // things like Ok(), BadRequest()
    {
        private readonly AuthService _authService;// Dependency Injection. DI container sees the builder.Services registrations and gives the scope

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            bool registered = await _authService.RegisterAsync(request);

            if (!registered)
                return BadRequest(new { message = "Email is already registered." });

            return Ok(new { message = "Registration successful." });
        }

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

        [Authorize]// checks the signature, issuer, audience and expiry. If authentication fails VerifyToken method is not executed
        [HttpGet("verify")]// If the token is valid, then [Authorize] allows to proceed with the request
        public IActionResult VerifyToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);// reads information stored inside the token
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                userId,
                role
            });
        }
    }
}