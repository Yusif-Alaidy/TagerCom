namespace TagerCom.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }

        // 👇 Relationship with User
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
