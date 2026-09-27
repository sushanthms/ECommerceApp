using ECommerceBackend.Data;
using ECommerceBackend.Models;

namespace ECommerceBackend
{
    public static class AdminSeeder
    {
        public static void SeedAdmin(AppDbContext context, IConfiguration configuration)
        {
            // Check whether an Admin already exists
            var adminExists = context.Users
                .Any(u => u.Role == "Admin");

            if (adminExists)
            {
                return;
            }

            // Get admin credentials from User Secrets
            var adminEmail = configuration["Admin:Email"]?.Trim().ToLowerInvariant();
            var adminPassword = configuration["Admin:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword) || adminPassword.Length < 8)
            {
                throw new InvalidOperationException(
                    "Admin credentials are not configured (email required, password at least 8 characters)."
                );
            }

            // Create the first Admin
            var admin = new User
            {
                Name = "Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = "Admin"
            };

            context.Users.Add(admin);

            context.SaveChanges();
        }
    }
}