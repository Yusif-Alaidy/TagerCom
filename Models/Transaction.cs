namespace TagerCom.Models
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Cashback,
        Referral
    }

    public class Transaction
    {
        public Guid             Id          { get; set; } = Guid.NewGuid();
        public Guid             WalletId    { get; set; }
        public TransactionType  Type        { get; set; }  // deposit, withdrawal, cashback, referral
        public decimal          Amount      { get; set; }
        public string           Description { get; set; } = null!;
        public DateTime         CreatedAt   { get; set; } = DateTime.UtcNow;

        // Navigation
        public Wallet Wallet { get; set; } = null!;
    }
}
