using System.ComponentModel.DataAnnotations;

namespace ECommerceBackend.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}

// The Register DTO represents the data required when creating a new account.
// Login DTO represents the data required to log in.

// while registering if frontend form has role selection and if someone registers as role=admin, the backend does not receives that field
// The Login DTO gives only the fields that are necessary for login.

// Migrations are not mandatory for changing a database. They are a convenient, organized, version-controlled way to manage the database structure changes from the C# models.
// if we add a new column in the User model, we have to write sql commands.
// so using migration we can run dotnet ef migrations add AddPhoneNumber
// dotnet ef database update
// EF Core Migrations is a way of Entity Framework Core to keep the SQL Server database structure synchronized with the C# models.
// migrations fodler contaisn the blueprint of the database table
// we do dotnet ef database update when we change the database table structure not when we add a new user