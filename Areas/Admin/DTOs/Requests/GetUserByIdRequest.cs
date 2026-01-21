namespace TagerCom.Areas.Admin.DTOs.Requests
{
    public class GetUserByIdRequest
    {
        public string Id { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
    }
}
