namespace TagerCom.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public double Rating { get; set; }=0;
        public decimal RevenueShare { get; set; }
        public bool Approved { get; set; } = false;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Product>? Products { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
