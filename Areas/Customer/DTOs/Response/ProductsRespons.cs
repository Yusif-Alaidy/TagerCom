namespace TagerCom.Areas.Customer.DTOs.Response
{
    public class ProductsRespons
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid? VendorId { get; set; }
        public Guid? CategoryId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? TotalSold { get; set; }
    }
}
