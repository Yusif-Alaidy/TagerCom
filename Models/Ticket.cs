namespace TagerCom.Models
{
    public class Ticket
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public string   CustomerId  { get; set; } = null!;
        public string?  SupportId   { get; set; }
        public string   Subject     { get; set; } = null!;
        public string   Status      { get; set; } = "open";
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser  Customer    { get; set; } = null!;
        public ApplicationUser? Support     { get; set; }
    }
}
