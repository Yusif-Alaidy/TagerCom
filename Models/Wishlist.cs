using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagerCom.Models
{
    public class Wishlist
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]

        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId     { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ApplicationUser User     { get; set; } = null!;
        public Product         Product  { get; set; } = null!;
    }
}
