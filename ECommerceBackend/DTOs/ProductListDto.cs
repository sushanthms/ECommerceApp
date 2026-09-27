namespace ECommerceBackend.DTOs
{
    public class ProductListDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string Category { get; set; } = string.Empty;

        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}