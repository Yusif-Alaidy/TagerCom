using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagerCom.Models
{
    public class Favorite
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(450)]

        public string ApplicationUserId { get; set; } = null!;

        [Required]
        public Guid ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
