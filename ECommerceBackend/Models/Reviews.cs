using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Models;

[Index(nameof(UserId), nameof(ProductId), IsUnique = true)]
[Index(nameof(ProductId), nameof(CreatedAt))]
public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
