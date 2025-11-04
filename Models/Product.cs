using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace TagerCom.Models
{
    public class Product
    {
        public int Id { get; set; }
        public Guid VendorId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; } // ✅ Added SubCategoryId

        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int SalesCount { get; set; } = 0;


        // Navigation
        public ICollection<Review>? Reviews { get; set; }
        public Vendor? Vendor { get; set; }
        public Category? Category { get; set; }
        public SubCategory? SubCategory { get; set; } // ✅ Navigation property

    }
}
