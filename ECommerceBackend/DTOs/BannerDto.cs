using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.DTOs
{
    public class BannerDto
    {
        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ButtonText { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Link { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }
    }
}