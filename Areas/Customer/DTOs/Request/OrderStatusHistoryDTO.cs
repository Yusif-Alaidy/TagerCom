namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class OrderStatusHistoryDTO
    {
        public string Status { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }

    }
}
