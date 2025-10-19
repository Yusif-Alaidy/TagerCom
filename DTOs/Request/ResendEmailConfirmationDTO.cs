using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class ResendEmailConfirmationDTO
    {
        [Required]
        public string EmailOrUserName { get; set; } = string.Empty;
    }
}
