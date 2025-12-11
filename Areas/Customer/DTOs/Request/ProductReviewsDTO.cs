using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class ProductReviewsDTO
    {

        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50.")]
        public int PageSize { get; set; } = 10;
    }
}
