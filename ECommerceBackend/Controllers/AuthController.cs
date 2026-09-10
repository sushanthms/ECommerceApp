using BCrypt.Net;
using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;// ASP.NET Core's web framework tools. ControllerBase, [ApiController], IActionResult
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerceBackend.Controllers
{
    [ApiController]// automatically gives response like 400 based on the data input.
    [Route("api/[controller]")]
    public class AuthController : ControllerBase // things like Ok(), BadRequest()
    {
        private readonly AppDbContext _context;// dependency injection. Declares a private field called _context to hold a reference to the database.
        private readonly IConfiguration _configuration;
        private readonly IApplicationLogger _logger;

        public AuthController(AppDbContext context, IConfiguration configuration, IApplicationLogger logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        // Registration of User
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)// IActionResult knows how to produce/send the HTTP response. describes what should be sent
        {
           try { 
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            // searches through the table, and gives the first matching row or null if no row matches.
            if (existingUser != null)// if a user is alreday registered existingUser stores taht otherwise it stores null. if existingUser is not null then that user is present
            {
                await _logger.LogMessageAsync("User registration failed - email already registered");
                return BadRequest(new
                    {
                        message = "Email is already registered."
                    });
                }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();
            await _logger.LogMessageAsync("User registration successful");

            return Ok(new
            {
                message = "Registration successful."
            });
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync("User registration failed - unexpected error");
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                await _logger.LogMessageAsync("User login failed");
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!passwordValid)
            {
                await _logger.LogMessageAsync("User login failed");
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }
            // building the access token
            var claims = new[]
                {
                    // new Claim(type,value)
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );
            // creating the token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);// token is an in-memory object, so converting it to jwt string
            await _logger.LogMessageAsync("User login successful");

            return Ok(new
            {
                message = "Login successful.",
                token = jwt,
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                role = user.Role
            });
        }

        [HttpGet("user-test")]
        [Authorize(Roles = "User")]
        public IActionResult UserTest()
        {
            return Ok(new
            {
                message = "User access successful."
            });
        }

        [HttpGet("admin-test")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminTest()
        {
            return Ok(new
            {
                message = "Admin access successful."
            });
        }

    }
}