namespace TagerCom.Models
{
    public class TicketUpdate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public string ActorId { get; set; } = null!;
        public ApplicationUser Actor { get; set; } = null!;

        public string? Message { get; set; }
        public bool IsInternal { get; set; } = false;

        //  (Snapshot)
        public Ticket.TicketStatus? OldStatus { get; set; }
        public Ticket.TicketStatus? NewStatus { get; set; }

        // (Snapshot)
        public string? OldSupportId { get; set; }

        public List<string> Attachments { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
