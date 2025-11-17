namespace TagerCom.Areas.Customer.DTOs.Response
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VendorName { get; set; } = string.Empty;

        public string CurrentStatus { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new();

    }
}
