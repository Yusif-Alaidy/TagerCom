using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Models
{
    public enum VendorStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
    public class Vendor
    {

        public Guid     Id                      { get; set; } = Guid.NewGuid();
        public string   ApplicationUserId       { get; set; } = string.Empty;
        public string   CompanyName             { get; set; } = null!;
        public decimal  Rating                  { get; set; } = 0m;
        public decimal  RevenueShare            { get; set; } // Vendor's revenue share percentage (0.15 = 15%)
        public bool     Approved                { get; set; } = false;
        [EnumDataType(typeof(VendorStatus))]    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VendorStatus Status              { get; set; } = VendorStatus.Pending;
        public DateTime  CreatedAt              { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt              { get; set; }

        // Navigation
        public ICollection<Product> Products            { get; set; } = new List<Product>();
        public ICollection<Order>   Orders              { get; set; } = new List<Order>();
        public ApplicationUser      ApplicationUser     { get; set; } = null!;
    }
}
