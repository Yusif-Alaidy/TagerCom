namespace TagerCom.DTOs.Response
{
    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;

        public string ImageUrl { get; set; }
        public string VendorName { get; set; }

        public string VendorID { get; set; }

    }
}
