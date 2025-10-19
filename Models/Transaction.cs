namespace TagerCom.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public string Type { get; set; } = null!; // deposit, withdrawal, cashback, referral
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Wallet Wallet { get; set; } = null!;
    }
}
