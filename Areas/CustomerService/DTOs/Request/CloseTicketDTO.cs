namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class CloseTicketDTO
    {

        public string ResolutionNotes { get; set; } = string.Empty;

        // Survey
        public int? SatisfactionRating { get; set; }     // 1..5 (اختياري)

        public bool Archive { get; set; } = true;        // default يقفل + يأرشف

    }
}
