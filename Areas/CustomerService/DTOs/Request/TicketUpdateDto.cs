using static TagerCom.Models.Ticket;

namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class TicketUpdateDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsInternal { get; set; }
        public string? Message { get; set; }

        public TicketStatus? OldStatus { get; set; }
        public TicketStatus? NewStatus { get; set; }

        public string? OldSupportId { get; set; }
        public string? NewSupportId { get; set; }


        public ActorDto? Actor { get; set; }


    }
}
