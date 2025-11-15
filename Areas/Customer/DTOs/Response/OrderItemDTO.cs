using TagerCom.Areas.Customer.DTOs.Request;

namespace TagerCom.Areas.Customer.DTOs.Response
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
        public string Description { get; set; } = null!;
        public List<OrderStatusHistoryDTO> StatusHistory { get; set; }

    }
}
