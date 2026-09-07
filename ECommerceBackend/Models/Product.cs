using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Models
{
    // An index is a database structure that speeds up lookups/searches on a column. It is used to make certain database operations faster. Uses B-tree structure.
    [Index(nameof(SKU), IsUnique = true)]
    public class Product
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
            
        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string Category { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
    }
}