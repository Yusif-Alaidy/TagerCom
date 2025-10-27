using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class UpdateInformationRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? SecondPhoneNumber { get; set; }
        public IFormFile? ProfileImgUrl { get; set; }

    }
}
