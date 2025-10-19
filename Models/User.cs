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
        public Cart? Cart { get; set; }
        public Vendor? Vendor { get; set; }
        public Wallet? Wallet { get; set; }
    }
}
