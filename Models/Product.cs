using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace TagerCom.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Vendor Vendor { get; set; } = null!;
        public Category? Category { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
