using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class ResetPasswordDTO
    {
        [Required]
        public string OTPNumber { get; set; } = string.Empty;
    }
}
