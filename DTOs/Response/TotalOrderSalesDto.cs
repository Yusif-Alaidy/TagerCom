namespace TagerCom.DTOs.Response
{
    public class TotalOrderSalesDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public CustomerDto Customer { get; set; } = new CustomerDto();
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();



    }
}
