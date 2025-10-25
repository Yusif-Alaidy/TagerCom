using System.Transactions;

namespace TagerCom.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; } = 0;

        // Navigation
        public ApplicationUser User { get; set; }
    }
}
