using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.DTOs
{
    public class AddReviewDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}