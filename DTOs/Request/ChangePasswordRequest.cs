using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class ChangePasswordRequest
    {
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
