using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Store.DTOs
{
    public class ProductsRequest
    {

        [Required]
        public Guid CategoryId { get; set; } 
       
        public Guid? BrandId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [Required]
        public IFormFile ImageUrl { get; set; } = null!;
    }
}
