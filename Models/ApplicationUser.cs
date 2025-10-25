using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TagerCom.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string? Name { get; set; }= string.Empty;
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        // Relationships
        public Vendor? Vendor { get; set; }
    }

}
