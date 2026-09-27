using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.Models.DTOs
{
    public class ProductCsvDto
    {
        [Required, MaxLength(50)]
        public string? SKU { get; set; }

        [Required, MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Range(0, 10000000)]
        public decimal Price { get; set; }

        [Range(0, 1000000)]
        public int Stock { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(2000)]
        public string? ImageFiles { get; set; }
    }
}