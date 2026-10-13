using ECommerceBackend.Data;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace ECommerceBackend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;// dependency injection. Declares a private field called _context to hold a reference to the database.
        private readonly IConfiguration _configuration;
        private readonly IApplicationLogger _logger;
        private const int AccessTokenMinutes = 1;
        private const int RefreshTokenDays = 7;
        private static string GenerateRefreshToken() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        // creating an access token during login and refresh
        private string CreateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(AccessTokenMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Builds the entity but does NOT save it. The caller decides when to call SaveChanges.
        private static (string Raw, RefreshToken Entity) BuildRefreshToken(int userId)
        {
            var raw = GenerateRefreshToken();
            var entity = new RefreshToken
            {
                UserId = userId,
                TokenHash = HashToken(raw),
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays)
            };
            return (raw, entity);
        }

        public AuthService(AppDbContext context, IConfiguration configuration, IApplicationLogger logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> RegisterAsync(RegisterDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            // searches through the table, and gives the first matching row, if no row matches it gives null.
            if (existingUser != null)
            {
                await _logger.LogMessageAsync($"User registration failed - email already registered: {email}");
                return false;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name,
                Email = email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                await _logger.LogMessageAsync($"User registration failed - email already registered (race): {email}");
                return false;
            }

            await _logger.LogMessageAsync($"User registration successful - UserId: {user.Id}, Email: {user.Email}");
            return true;
        }

        public async Task<AuthResult?> LoginAsync(LoginDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                await _logger.LogMessageAsync($"User login failed - no account found for Email: {email}");
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _logger.LogMessageAsync($"User login failed - incorrect password for Email: {email}, UserId: {user.Id}", "Warning");
                return null;
            }

            var accessToken = CreateAccessToken(user);
            var (rawRefresh, refreshEntity) = BuildRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshEntity);
            await _context.SaveChangesAsync();

            await _logger.LogMessageAsync($"User login successful - UserId: {user.Id}, Email: {user.Email}, Role: {user.Role}");

            return new AuthResult(accessToken, rawRefresh, user);
        }

        // Takes the refresh token from the request, verifies that it is valid, then issues a new access token and a new refresh token.
        public async Task<AuthResult?> RefreshAsync(string rawToken)
        {
            var hash = HashToken(rawToken);

            var stored = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored == null)
            {
                await _logger.LogMessageAsync("Token refresh failed - unknown token", "Warning");
                return null;
            }

            // Checking whether this token(this row) was replaced by a new refresh token. Checking whether this token was already used to get another refresh token
            // now user is having token b, token a is already revoked to token b. but the request came form token a so the if block became true
            if (stored.RevokedAt != null && stored.ReplacedByTokenHash != null)
            {// gives value for the current token revoked field(which was null)
                await RevokeAllForUserAsync(stored.UserId);
                await _logger.LogMessageAsync($"Refresh token reuse detected - all tokens revoked for UserId: {stored.UserId}", "Warning");
                return null;
            }
            // the code reaches here when previous token was expired but not revoked(it should happen by code operation), so we check whether it is active
            // now token b is expired and revoked is not marked with time value, so isActive has value false, so if block is true
            // this happens when user did not login for more than 7 days. if (stored.RevokedAt != null && stored.ReplacedByTokenHash != null) is false
            if (!stored.IsActive)
            {
                return null;
            }

            // Rotate: revoking the old token, creating the new one. updating the revoke value of old token
            var (rawNew, newEntity) = BuildRefreshToken(stored.UserId);

            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByTokenHash = newEntity.TokenHash;
            _context.RefreshTokens.Add(newEntity);// newEntity is a new row. userId, TokenHash, ExpiresAt are added to that row and remaining values are empty in that row
            await _context.SaveChangesAsync();

            // stored.User was loaded fresh from the DB, so the role is current.
            var accessToken = CreateAccessToken(stored.User);
            return new AuthResult(accessToken, rawNew, stored.User);
        }

        private async Task RevokeAllForUserAsync(int userId)
        {
            await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
        }

        public async Task LogoutAsync(string rawToken)
        {
            var hash = HashToken(rawToken);
            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored != null && stored.IsActive)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await _logger.LogMessageAsync($"User logout - UserId: {stored.UserId}");
            }
        }

        public record AuthResult(string AccessToken, string RefreshToken, User User);
    }
}