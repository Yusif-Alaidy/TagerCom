namespace TagerCom.Models
{
    public class UserOTP
    {
        public Guid     Id { get; set; } = Guid.NewGuid();
        public string   ApplicationUserId { get; set; } = null!;
        public string   OTPNumber { get; set; } = null!;
        public DateTime ValidTo { get; set; }
        public bool     IsUsed { get; set; } = false;

        // Navigation
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
