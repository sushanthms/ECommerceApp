using ECommerceBackend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;// ASP.NET Core's web framework tools. ControllerBase, [ApiController], IActionResult
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;

namespace ECommerceBackend.Controllers
{
    [ApiController]// http responses, model binding and validation
    [Route("api/[controller]")]
    public class AuthController : ControllerBase// things like Ok(), BadRequest()
    {
        private readonly AuthService _authService;// Dependency Injection. DI container sees the builder.Services registrations and gives the scope
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private const string RefreshCookieName = "refreshToken";

        private void SetRefreshCookie(string token)
        {
            Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,// sends only in https 
                SameSite = SameSiteMode.None,
                Path = "/api/auth",// sends this cookie to requests whose path starts with /api/auth
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        private void DeleteRefreshCookie()
        {
            Response.Cookies.Delete(RefreshCookieName, new CookieOptions
            {
                Path = "/api/auth",
                Secure = true,
                SameSite = SameSiteMode.None
            });
        }

        public AuthController(AuthService authService, IValidator<RegisterDto> registerValidator, IValidator<LoginDto> loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var validation = await _registerValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed.",
                    errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                });
            }

            bool registered = await _authService.RegisterAsync(request);

            if (!registered)
                return BadRequest(new { message = "Email is already registered." });

            return Ok(new { message = "Registration successful." });
        }

        [EnableRateLimiting("login")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var validation = await _loginValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed.",
                    errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                });
            }

            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            SetRefreshCookie(result.RefreshToken);

            return Ok(new
            {
                message = "Login successful.",
                token = result.AccessToken,
                userId = result.User.Id,
                name = result.User.Name,
                email = result.User.Email,
                role = result.User.Role
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var rawToken = Request.Cookies[RefreshCookieName];

            if (string.IsNullOrEmpty(rawToken))
                return Unauthorized(new { message = "No refresh token." });

            var result = await _authService.RefreshAsync(rawToken);

            if (result == null)
            {
                DeleteRefreshCookie();
                return Unauthorized(new { message = "Session expired. Please log in again." });
            }

            SetRefreshCookie(result.RefreshToken);

            return Ok(new
            {
                token = result.AccessToken,
                userId = result.User.Id,
                name = result.User.Name,
                email = result.User.Email,
                role = result.User.Role
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var rawToken = Request.Cookies[RefreshCookieName];

            if (!string.IsNullOrEmpty(rawToken))
                await _authService.LogoutAsync(rawToken);

            DeleteRefreshCookie();
            return Ok(new { message = "Logged out." });
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