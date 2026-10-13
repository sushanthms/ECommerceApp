using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.Models.DTOs
{
    public class AddressDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Line1 { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Line2 { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits.")]
        public string Pincode { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}