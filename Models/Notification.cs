namespace TagerCom.Models
{
    public class Notification
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public string   UserId      { get; set; }
        public string   Message     { get; set; } = null!;
        public bool     IsRead      { get; set; } = false;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
