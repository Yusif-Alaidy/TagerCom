using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TagerCom.Models
{
    public class Points
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public int TotalPoints { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
