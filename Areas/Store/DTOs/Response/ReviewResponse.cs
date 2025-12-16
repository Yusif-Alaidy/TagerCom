namespace TagerCom.Areas.Store.DTOs.Response
{
    public class ReviewResponse
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
