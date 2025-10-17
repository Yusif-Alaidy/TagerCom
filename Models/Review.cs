namespace TagerCom.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Product Product { get; set; } = null!;
        public User Customer { get; set; } = null!;
    }
}
