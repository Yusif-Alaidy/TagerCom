using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class VendorRegisterRequest
    {
        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Company name must be between 3 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Company name can only contain letters, numbers, spaces, hyphens, and underscores.")]
        public string CompanyName { get; set; }
    }
}
