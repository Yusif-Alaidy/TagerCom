namespace TagerCom.Models
{
    public class SubCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        // العلاقة مع Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // العلاقة مع Products
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
