namespace TagerCom.Areas.Store.DTOs.Response
{
    public class OrdersResponse
    {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string? Customer { get; set; } 
            public string? Store { get; set; }
            public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
            public decimal? TotalAmount { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  
        
    }
}
