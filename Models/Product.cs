using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace TagerCom.Models
{
    public class Product
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public Guid?    StoreId     { get; set; }
        public Guid     CategoryId  { get; set; }
        public Guid?    BrandId     { get; set; }
        public string   Name        { get; set; } = null!;
        public string   Description { get; set; } = null!;
        public decimal  Price       { get; set; }
        public int      Stock       { get; set; }
        public string?  ImageUrl    { get; set; }
        public bool     IsActive    { get; set; } = true;
        public bool     IsDeleted   { get; set; } = false;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // Discount ==========================================================================
        public decimal?  DiscountValueFixed { get; set; }
        public DateTime? DiscountStartDate  { get; set; }
        public DateTime? DiscountEndDate    { get; set; }

        // Navigation ========================================================================
        public ICollection<Review>      Reviews    { get; set; } = new List<Review>();
        public ICollection<CartItem>    CartItems  { get; set; } = new List<CartItem>();
        public ICollection<OrderItem>   OrderItems { get; set; } = new List<OrderItem>();
        public Store?                   Store      { get; set; } = null!;
        public Category                 Category   { get; set; } = null!;
        public Brand                    Brand      { get; set; } = null!;
    }
}
