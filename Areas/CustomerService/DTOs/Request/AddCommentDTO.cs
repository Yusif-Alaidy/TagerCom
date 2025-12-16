namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class AddCommentDTO
    {
        public string Comment { get; set; } = string.Empty;
        public List<string> Attachments { get; set; } = new();

    }
}
