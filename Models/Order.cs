using System.ComponentModel.DataAnnotations;

namespace TagerCom.Models
{
    public class Order
    {
        public Guid     Id { get; set; } = Guid.NewGuid();
        public string?  ApplicationUserId { get; set; }
        public Guid?    VendorId { get; set; }
        public string   Status { get; set; } = "pending";
        public decimal  TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<OrderItem> OrderItems        { get; set; } = new List<OrderItem>();
        public ApplicationUser?       ApplicationUser   { get; set; }
        public Vendor?                Vendor            { get; set; }
        public Payment?               Payment           { get; set; }
    }
}
