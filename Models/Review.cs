using System.ComponentModel.DataAnnotations.Schema;

namespace TagerCom.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation


        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;
        [ForeignKey(nameof(CustomerId))]
        public ApplicationUser Customer { get; set; } = null!;
    }
}
