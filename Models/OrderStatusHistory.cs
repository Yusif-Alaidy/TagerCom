public class OrderStatusHistory
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
}
