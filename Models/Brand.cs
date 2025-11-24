namespace TagerCom.Models
{
    public class Brand
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string  BrandName { get; set; }

        // Navigartion ------------------------------
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
