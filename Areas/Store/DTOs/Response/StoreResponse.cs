using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Areas.Store.DTOs.Response
{
    public class StoreResponse
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string? ApplicationUserId { get; set; } = string.Empty;
        public string StoreName { get; set; } = null!;
        public decimal Rating { get; set; } = 0m;
        public int RevenueShare { get; set; } = 15; // Vendor's revenue share percentage (0.15 = 15%)
        public bool IsActive { get; set; } = false;
        [EnumDataType(typeof(StoreStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StoreStatus Status { get; set; } = StoreStatus.Pending;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
