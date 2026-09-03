namespace ECommerceBackend.Models.DTOs
{
    public class ProductCsvDto
    {
        public string? SKU { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
    }
}