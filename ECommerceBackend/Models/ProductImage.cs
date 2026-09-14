using System.Text.Json.Serialization;

namespace ECommerceBackend.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        [JsonIgnore]
        public Product Product { get; set; } = null!;
    }
}