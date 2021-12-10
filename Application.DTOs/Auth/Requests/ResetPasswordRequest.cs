
using System.ComponentModel.DataAnnotations;

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string Token { get; set; }
}

