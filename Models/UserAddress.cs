namespace TagerCom.Models
{
    public class UserAddress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Label { get; set; } = "Home"; // Home, Work, etc.
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? ZipCode { get; set; }
        public bool IsDefault { get; set; } = false;

        // Navigation
        public User User { get; set; } = null!;
    }
}
