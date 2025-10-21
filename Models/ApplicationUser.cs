using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TagerCom.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string? Name { get; set; }= string.Empty;

        public string? City { get; set; } 

        public string? street { get; set; } 
        public string? PostalCode { get; set; } 

        public List<RefreshToken> RefreshTokens { get; set; } = new();

    }
}
