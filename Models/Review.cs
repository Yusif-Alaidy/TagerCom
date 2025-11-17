namespace TagerCom.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Product Product { get; set; } = null!;
        public ApplicationUser Customer { get; set; } = null!;
    }
}
