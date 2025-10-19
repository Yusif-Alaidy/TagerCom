namespace TagerCom.Models
{
    public class UserOTP
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string OTPNumber { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
