public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
}
