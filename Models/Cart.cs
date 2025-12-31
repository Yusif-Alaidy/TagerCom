namespace TagerCom.Models
{
    public class Cart
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public string   UserId      { get; set; } = null!;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser User        { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
