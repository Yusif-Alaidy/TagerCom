using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Store.DTOs.Request
{
    public class ProductsUpdateRequest
    {
        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        public IFormFile? ImageUrl { get; set; }
    }
}
