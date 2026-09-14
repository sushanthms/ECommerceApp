using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.DTOs
{
    public class SendOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}