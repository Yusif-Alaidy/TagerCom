using static TagerCom.Models.Ticket;

namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class UpdateTicketDTO
    {
        public TicketStatus? Status { get; set; }
        public Priority? Priority { get; set; }
        public string? SupportId { get; set; }
        public string? Note { get; set; }
        public bool IsInternal { get; set; } = false;

    }
}
