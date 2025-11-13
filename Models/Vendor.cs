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

        public Guid Id                                  { get; set; }
        public string Name { get; set; }
        // Relation with User
        public string ApplicationUserId                 { get; set; }
        public ApplicationUser ApplicationUser          { get; set; }

        // Business Info
        public string CompanyName                       { get; set; } = null!;
        public decimal Rating                           { get; set; } = 0m;
        /// <summary>
        /// Vendor's revenue share percentage (0.15 = 15%)
        /// </summary>
        public decimal RevenueShare                     { get; set; }
        public bool Approved                            { get; set; } = false;
        [EnumDataType(typeof(VendorStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VendorStatus Status                      { get; set; } = VendorStatus.Pending;

        // Audit Fields
        public DateTime CreatedAt                       { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt                      { get; set; }

        // Navigation
        public List<Product>? Products                  { get; set; }
    }
}
