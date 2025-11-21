using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TagerCom.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string?  ProfileImgUrl       { get; set; } = "default.jpg";
        public string?  FirstName           { get; set; } = string.Empty;
        public string?  LastName            { get; set; } = string.Empty;
        public string?  PhoneNumber         { get; set; } = string.Empty;
        public string?  SecondPhoneNumber   { get; set; } = string.Empty;

        // Navigation
        public ICollection<UserAddress> userAddresses   { get; set; } = new List<UserAddress>();
        public ICollection<Review>      Reviews         { get; set; } = new List<Review>();
        public Cart                     Cart            { get; set; }
        public ICollection<Order>       Orders          { get; set; } = new List<Order>();
        public Vendor?                  Vendor          { get; set; }
    }

}
