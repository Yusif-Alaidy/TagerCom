using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagerCom.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]

        public string ApplicationUserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}
