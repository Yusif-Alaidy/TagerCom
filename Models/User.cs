using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Net.Sockets;

namespace TagerCom.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public ICollection<UserAddress>? Addresses { get; set; }
        public Cart? Cart { get; set; }
        public Vendor? Vendor { get; set; }
        public Wallet? Wallet { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<Ticket>? Tickets { get; set; }
    }
}
