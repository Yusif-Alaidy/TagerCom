namespace TagerCom.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public Guid VendorId { get; set; }
        public string Status { get; set; } = "pending";
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation

        public ApplicationUser Customer { get; set; } = null!;
        public Vendor Vendor { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}
