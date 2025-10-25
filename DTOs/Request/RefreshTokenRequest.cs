using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
