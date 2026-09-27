using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(72)]
    public string Password { get; set; } = string.Empty;
}