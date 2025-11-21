namespace TagerCom.Models
{
    public class RefreshToken
    {
        public Guid     Id      { get; set; } = Guid.NewGuid();
        public string   UserId  { get; set; } = string.Empty;
        public string   Token   { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public bool     IsExpired => DateTime.UtcNow >= Expires;

        // Navigation
        public ApplicationUser  User    { get; set; }
    }
}
