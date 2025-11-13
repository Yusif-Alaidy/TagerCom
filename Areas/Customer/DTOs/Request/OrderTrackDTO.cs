using TagerCom.Areas.Customer.DTOs.Response;

namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class OrderTrackDTO
    {

        public int Id { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public List<OrderItemDTO> Items { get; set; } = new();
        public List<OrderStatusHistoryDTO> StatusHistory { get; set; } = new();

    }
}
