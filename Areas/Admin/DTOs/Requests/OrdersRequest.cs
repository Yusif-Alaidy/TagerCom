using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Areas.Admin.DTOs.Requests
{
    public class OrdersRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? CustomerId { get; set; } = string.Empty;
        public Guid? StoreId { get; set; }
        [EnumDataType(typeof(StoreStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus OrderStatus { get; set; } 
        public decimal? TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        
    }
}
