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
        public int ProductId { get; set; }// It defines what data is expected when adding something to the cart.
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
    }
}