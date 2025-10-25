namespace TagerCom.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? SupportId { get; set; }
        public string Subject { get; set; } = null!;
        public string Status { get; set; } = "open";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        //public ApplicationUser Customer { get; set; } = null!;
        public ApplicationUser? Support { get; set; }
    }
}
