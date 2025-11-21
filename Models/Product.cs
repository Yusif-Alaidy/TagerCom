using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace TagerCom.Models
{
    public class Product
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public Guid     VendorId    { get; set; }
        public Guid?    CategoryId  { get; set; }
        public string   Name        { get; set; } = null!;
        public string   Description { get; set; } = null!;
        public decimal  Price       { get; set; }
        public int      Stock       { get; set; }
        public string?  ImageUrl    { get; set; }
        public bool     IsActive    { get; set; } = true;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Review> Reviews      { get; set; } = new List<Review>();
        public ICollection<CartItem> CartItems  { get; set; } = new List<CartItem>();
        public Vendor? Vendor                   { get; set; }
        public Category? Category               { get; set; }
    }
}
