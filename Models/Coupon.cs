using System.ComponentModel.DataAnnotations;
namespace TagerCom.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        [Required]
        public string Code { get; set; } = null!;
        public decimal DiscountPercentage { get; set; } // e.g., 10 = 10%
        public DateTime ExpirationDate { get; set; }
        public int UsageLimit { get; set; } = 1;
        public int TimesUsed { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public int? VendorId { get; set; } // Optional if coupon is vendor-specific
    }
}
