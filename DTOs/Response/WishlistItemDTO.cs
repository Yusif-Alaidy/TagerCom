namespace TagerCom.DTOs.Response
{
    public class WishlistItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductDescription { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
