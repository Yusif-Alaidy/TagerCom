using static TagerCom.Models.Complaint;

namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class GetComplaintsQueryDTO
    {
        public ComplaintStatus? Status { get; set; }
        public Guid? VendorId { get; set; }
        public ComplaintType? Type { get; set; }
        public bool? HighPriority { get; set; }
        public bool? OverdueOnly { get; set; }

        public int CurrentPage { get; set; } = 1; // ✅ تبعت ده بس
    }
}
