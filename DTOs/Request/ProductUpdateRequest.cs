namespace TagerCom.DTOs.Request
{
    public class ProductUpdateRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int VendorId { get; set; }
        public int? CategoryId { get; set; }
    }
}
