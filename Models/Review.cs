using System.ComponentModel.DataAnnotations;

namespace TagerCom.Models
{
    public class Review
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public Guid     ProductId   { get; set; }
        public string   CustomerId  { get; set; } = string.Empty;
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int      Rating      { get; set; }
        public string?  Comment     { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // Navigation
        public Product          Product { get; set; } = null!;
        public ApplicationUser  Customer { get; set; } = null!;
    }
}
