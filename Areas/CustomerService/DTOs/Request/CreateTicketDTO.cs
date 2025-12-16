using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static TagerCom.Models.Ticket;

namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class CreateTicketDTO
    {
        public Guid? OrderId { get; set; }

        [Required]
        public TicketType Type { get; set; } = TicketType.OrderIssue;

        public Priority Priority { get; set; } = Priority.Medium;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string IssueDescription { get; set; } = string.Empty;

        public List<IFormFile>? Attachments { get; set; }
    }
}
