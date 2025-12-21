using System.Text.Json.Serialization;

namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class UpdateOrderStatusRequest
    {

       [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }

        public string? IssueDescription { get; set; }
    }
}
