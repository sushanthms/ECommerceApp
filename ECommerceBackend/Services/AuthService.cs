using BCrypt.Net;
using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace ECommerceBackend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;// dependency injection. Declares a private field called _context to hold a reference to the database.
        private readonly IConfiguration _configuration;
        private readonly IApplicationLogger _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, IApplicationLogger logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // Registration of User
        public async Task<bool> RegisterAsync(RegisterDto request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            // searches through the table, and gives the first matching row or null if no row matches.
            if (existingUser != null)// if a user is alreday registered existingUser stores taht otherwise it stores null. if existingUser is not null then that user is present
            {
                await _logger.LogMessageAsync($"User registration failed - email already registered: {request.Email}");
                return false;
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
            await _logger.LogMessageAsync($"User registration successful - UserId: {user.Id}, Email: {user.Email}");

            return true;
        }

        // Login of User and Admin
        public async Task<(bool Success, string? Token, User? User)> LoginAsync(LoginDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                await _logger.LogMessageAsync($"User login failed - no account found for Email: {request.Email}");
                return (false, null, null);
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                await _logger.LogMessageAsync($"User login failed - incorrect password for Email: {request.Email}, UserId: {user.Id}", "Warning");
                return (false, null, null);
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

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // creating the token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);// token is an in-memory object, so converting it to jwt string
            await _logger.LogMessageAsync($"User login successful - UserId: {user.Id}, Email: {user.Email}, Role: {user.Role}");

            return (true, jwt, user);
        }
    }
}