using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.Models
{
    public class Address
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Line1 { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Line2 { get; set; }

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}