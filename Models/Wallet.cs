using System.Transactions;

namespace TagerCom.Models
{
    public class Wallet
    {
        public Guid     Id      { get; set; } = Guid.NewGuid();
        public String   UserId  { get; set; } = String.Empty;
        public decimal  Balance { get; set; } = 0;
        public int      Point   { get; set; } 

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
