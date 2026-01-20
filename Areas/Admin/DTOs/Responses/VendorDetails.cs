namespace TagerCom.Areas.Admin.DTOs.Responses
{
    public class VendorDetails
    {
        public string Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
