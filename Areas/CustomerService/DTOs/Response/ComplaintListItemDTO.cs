using static TagerCom.Models.Complaint;

namespace TagerCom.Areas.CustomerService.DTOs.Response
{
    public class ComplaintListItemDTO
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? VendorId { get; set; }

        public string Subject { get; set; } = "";
        public ComplaintType Type { get; set; }
        public ComplaintPriority Priority { get; set; }
        public ComplaintStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        //   ============ SLA tracking =============  

        public DateTime DueAt { get; set; }
        public int SlaRemainingMinutes { get; set; }

        //   ============ Overdue =============  
        public bool IsOverdue { get; set; }
        public int OverdueByMinutes { get; set; }
        public string OverdueByText { get; set; } = "";
    }
}
