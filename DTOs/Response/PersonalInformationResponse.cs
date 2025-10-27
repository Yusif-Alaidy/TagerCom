using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Response
{
    public class PersonalInformationResponse
    {
        public string Id { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public List<UserAddress> userAddresses { get; set; }
    }
}
