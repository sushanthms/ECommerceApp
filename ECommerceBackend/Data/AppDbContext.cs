using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }// DbSet means table with User objects

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
}
// Users is the collection table in SSMS
// User is the C# model

// Install-Package CsvHelper