namespace TagerCom.Models
{
    public class Ticket
    {

        public enum TicketStatus
        {
            Open,
            InProgress,
            Resolved

        }

        public enum Priority
        {
            High,
            Low,
            Medium

        }

        public enum TicketType
        {
            OrderIssue,
            PaymentProblem,
            ShippingDelay,
            ProductQuality,
            RefundRequest,
            GeneralInquiry
        }

        public Guid     Id          { get; set; } = Guid.NewGuid();
        public Guid? OrderId { get; set; }

        public string   CustomerId  { get; set; } = null!;
        public string?  SupportId   { get; set; }
        public string   Subject     { get; set; } = null!;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public TicketStatus status { get; set; } = TicketStatus.Open;
        public Priority priority { get; set; } = Priority.Medium;

        public string IssueDescription { get; set; } = null!;
        public TicketType Type { get; set; }
        public string? ResolutionNotes { get; set; }

        public int? SatisfactionRating { get; set; }
        public bool IsArchived { get; set; } = false;


        // Navigation

        public Order? Order { get; set; }
        public ICollection<TicketUpdate> Updates { get; set; } = new List<TicketUpdate>();
        public List<string> Attachments { get; set; } = new();

        public ApplicationUser  Customer    { get; set; } = null!;
        public ApplicationUser? Support     { get; set; }
    }
}
