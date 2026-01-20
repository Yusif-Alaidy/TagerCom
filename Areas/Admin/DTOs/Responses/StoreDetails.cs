namespace TagerCom.Areas.Admin.DTOs.Responses
{
    public class StoreDetails
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; } = null!;
        public StoreStatus Status { get; set; }   // Or Enum if you prefer
        public bool IsActive { get; set; }
        public decimal Rating { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public DateTime RegisteredAt { get; set; }

    }
}
