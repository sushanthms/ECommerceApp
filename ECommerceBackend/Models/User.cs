using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Models // belongs to Models folder
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty; // creates name = "" instead of name = Null

        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";// User is the default value

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
