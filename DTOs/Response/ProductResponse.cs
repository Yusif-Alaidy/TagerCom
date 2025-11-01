namespace TagerCom.DTOs.Response
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
