using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class ResetPasswordDTO
    {
        [Required]
        public string OTPNumber { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
