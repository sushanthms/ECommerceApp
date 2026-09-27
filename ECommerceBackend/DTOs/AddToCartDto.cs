using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
        public int Quantity { get; set; }
    }
    public class AddToCartDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        [Range(0, 100)]
        public int Quantity { get; set; }
    }
}