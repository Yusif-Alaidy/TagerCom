namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class AddCommentDTO
    {
        public string Comment { get; set; } = string.Empty;
        public List<IFormFile> Attachments { get; set; } = new();

    }
}
