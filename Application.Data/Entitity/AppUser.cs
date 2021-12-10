
using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}
