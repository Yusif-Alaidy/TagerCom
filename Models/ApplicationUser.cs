using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TagerCom.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfileImgUrl { get; set; } = "default.jpg";
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; } = string.Empty;
        public string? SecondPhoneNumber { get; set; } = string.Empty;
        // Relationships
        public List<UserAddress> userAddresses { get; set; } = new();
        public Vendor? Vendor { get; set; }
    }

}
