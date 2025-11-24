using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Models
{
    public enum StoreStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
    public class Store
    {

        public Guid     Id                      { get; set; } = Guid.NewGuid();
        public string   ApplicationUserId       { get; set; } = string.Empty;
        public string   StoreName               { get; set; } = null!;
        public decimal  Rating                  { get; set; } = 0m;
        public int      RevenueShare            { get; set; } = 15; // Vendor's revenue share percentage (0.15 = 15%)
        public bool     IsActive                { get; set; } = false;
        [EnumDataType(typeof(StoreStatus))]    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StoreStatus Status              { get; set; } = StoreStatus.Pending;
        public DateTime  CreatedAt              { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt              { get; set; }

        // Navigation
        public ICollection<Product> Products            { get; set; } = new List<Product>();
        public ICollection<Order>   Orders              { get; set; } = new List<Order>();
        public ApplicationUser      ApplicationUser     { get; set; } = null!;
    }
}
