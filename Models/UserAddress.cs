namespace TagerCom.Models
{
    public class UserAddress
    {
        public Guid     Id                  { get; set; } = Guid.NewGuid();
        public string   ApplicationUserId   { get; set; } = null!;
        public string   Label               { get; set; } = "Home"; // Home, Work, etc.
        public string   Country             { get; set; } = null!;
        public string   City                { get; set; } = null!;
        public string   Street              { get; set; } = null!;
        public string?  ZipCode             { get; set; }
        public bool     IsDefault           { get; set; } = false;

        // Navigation
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
