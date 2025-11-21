namespace TagerCom.Models
{
    public class Payment
    {
        public Guid     Id              { get; set; } = Guid.NewGuid();
        public Guid     OrderId         { get; set; }
        public string   Method          { get; set; } = null!;
        public decimal  Amount          { get; set; }
        public string   Status          { get; set; } = "pending";
        public Guid?    TransactionId   { get; set; } = null!;
        public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order        Order       { get; set; } = null!;
        public Transaction? Transaction { get; set; }
    }
}
