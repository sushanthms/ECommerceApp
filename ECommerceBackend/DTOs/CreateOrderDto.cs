using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.DTOs
{
    public class CreateOrderDto
    {
        public int? AddressId { get; set; }
        public bool SaveAddress { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be 10 digits.")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits.")]
        public string Pincode { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = "Cash on Delivery";
    }
}