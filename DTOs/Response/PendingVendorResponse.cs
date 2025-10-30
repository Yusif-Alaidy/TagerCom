namespace TagerCom.DTOs.Response
{
    public class PendingVendorResponse
    {
        public string ApplicationUserId     { get; set; }
        public Guid vendoreId               { get; set; }
        public string Username              { get; set; }
        public string Email                 { get; set; }
        public string StoreName             { get; set; }
        public string phoneNumber           { get; set; }
        public string? SecondPhoneNumber    { get; set; }
        public String Status                { get; set; } 

        public DateTime CreatedAt           { get; set; }
    }
}
