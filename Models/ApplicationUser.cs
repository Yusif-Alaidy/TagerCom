using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TagerCom.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string Name { get; set; }= string.Empty;

        public string City { get; set; } = string.Empty;

        public string street { get; set; } = string.Empty;

        public string PostalCode { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new();

    }
}
