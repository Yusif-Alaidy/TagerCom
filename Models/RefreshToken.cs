namespace TagerCom.Models
{
    public class RefreshToken
    {
        
            public int Id { get; set; }
            public string Token { get; set; } = string.Empty;
            public DateTime Expires { get; set; }
            public DateTime Created { get; set; } = DateTime.UtcNow;
            public string CreatedByIp { get; set; } = string.Empty;
            public bool IsExpired => DateTime.UtcNow >= Expires;
            public string UserId { get; set; }
            public ApplicationUser User { get; set; }
        
    }
}
