using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class NewPasswordDTO
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
