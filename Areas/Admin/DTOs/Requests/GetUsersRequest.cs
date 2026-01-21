namespace TagerCom.Areas.Admin.DTOs.Requests
{
    public class GetUsersRequest
    {
        public string email     { get; set; } = string.Empty;
        public string userName  { get; set; } = string.Empty;
        public int page         { get; set; } = 1;
        public int pageSize     { get; set; } = 10;

    }
}
