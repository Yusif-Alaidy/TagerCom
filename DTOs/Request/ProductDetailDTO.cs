namespace TagerCom.DTOs.Request
{
    public class ProductDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public List<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();
    }

    public class ReviewDTO
    {
        public string Comment { get; set; }
    }
}
