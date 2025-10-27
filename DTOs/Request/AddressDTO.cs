using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class AddressDTO
    {
        public int UserId { get; set; }
        public string Label { get; set; } = "Home"; // Home, Work, etc.
        [Required]
        public string Country { get; set; } = null!;
        [Required]
        public string City { get; set; } = null!;
        [Required]
        public string Street { get; set; } = null!;
        public string? ZipCode { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
