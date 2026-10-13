using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Models
{
    [Index(nameof(TokenHash), IsUnique = true)]
    public class RefreshToken
    {// thsi file creates row for each refresh token
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }// at first time it has value as null, when stored.RevokedAt = DateTime.UtcNow; it gets a values so it means it is revoked
        // If this token was replaced, which new refresh token replaced it?
        public string? ReplacedByTokenHash { get; set; }// Stores the hash of the new refresh token that replaced the current refresh token.

        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    }
}