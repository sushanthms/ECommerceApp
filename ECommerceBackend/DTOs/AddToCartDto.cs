namespace ECommerceBackend.DTOs
{
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