namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class OrderStatusHistoryDTO
    {
        public OrderStatus Status { get; set; }
        public DateTime ChangedAt { get; set; }

    }
}
