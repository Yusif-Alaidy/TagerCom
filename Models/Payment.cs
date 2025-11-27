namespace TagerCom.Models
{
    public enum PaymentMethod
    {
        Visa,
        Cash,
    }
    public enum PaymentStatus
    {
        Pending,
        Completed,    
        Failed,       
        Refunded      
    }
    public class Payment
    {
        public Guid          Id              { get; set; } = Guid.NewGuid();
        public Guid          OrderId         { get; set; }
        public PaymentMethod Method          { get; set; } = PaymentMethod.Cash;
        public decimal       Amount          { get; set; }
        public PaymentStatus PaymentStatus   { get; set; } = PaymentStatus.Pending;
        public Guid?         TransactionId   { get; set; } = null!;
        public DateTime      CreatedAt       { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order        Order       { get; set; } = null!;
        public Transaction? Transaction { get; set; }
    }

}
