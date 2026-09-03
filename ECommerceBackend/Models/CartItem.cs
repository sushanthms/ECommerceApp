namespace ECommerceBackend.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;// cartitem and product are in same folder so cartitem can access product

        public int Quantity { get; set; }
    }
}