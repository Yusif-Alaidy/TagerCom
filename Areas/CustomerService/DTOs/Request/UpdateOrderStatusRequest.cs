namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class UpdateOrderStatusRequest
    {

        public OrderStatus Status { get; set; }

        public string? IssueDescription { get; set; }
    }
}
