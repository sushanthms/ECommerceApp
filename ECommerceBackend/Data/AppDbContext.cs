using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using Payment = ECommerceBackend.Payments.Models.Payment;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Data
{
    public class AppDbContext : DbContext
    {
        // AddDbContext knows how to register a class for the Dependency Injection process. <AppDbContext> is the class that is being registered.
        // When a constructor wants a service, the DI container calls AppDbContext's constructor, and the constructor runs
        public AppDbContext(DbContextOptions<AppDbContext> options)// constructor. DbContextOptions<AppDbContext> is the type. DbContextOptions are specifically for the AppDbContext class
            : base(options)// sends options to the parent(DbContext) 
        {
        }

        public DbSet<User> Users { get; set; }// DbSet means table with User objects. AppDbContext has access to the User entities/table.
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne<Product>().WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne<User>().WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // we have created, captured, failed, refunded. their indexes are 0, 1, 2, 3 so when the order is created, the status is 0. later we will change the status in the services
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.OrderId)
                .IsUnique()
                .HasFilter("[Status] = 0");
        }
    }
}