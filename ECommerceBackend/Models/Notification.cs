using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.Models
{
    public class Notification   
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;// LowStock

        [MaxLength(300)]
        public string Message { get; set; } = string.Empty;
        public int? ProductId { get; set; }// some notifications won't be about a product.
        public bool IsRead { get; set; }// false(default)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}