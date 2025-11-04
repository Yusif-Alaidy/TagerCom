namespace TagerCom.DTOs.Response
{
    public class ProductUpdateResponseDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string VendorId { get; set; } = null!;
        public string VendorName { get; set; } = null!;
    }
}
