namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class ProductRatingDTO
    {

        public Guid ProductId { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public List<string?> Comments { get; set; } = new();


    }
}
