namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class EscalateComplaintDTO
    {
        public string ManagerId { get; set; } = null!;
        public int UrgencyLevel { get; set; }           
        public DateTime Deadline { get; set; }          
        public string? Note { get; set; }               
    }
}
