namespace TagerCom.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Method { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "pending";
        public string TransactionId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order Order { get; set; } = null!;
    }
}
