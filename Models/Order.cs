namespace TagerCom.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int VendorId { get; set; }
        public string Status { get; set; } = "pending";
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ApplicationUser Customer { get; set; } = null!;
        public Vendor Vendor { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}
