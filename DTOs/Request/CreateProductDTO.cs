using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class CreateProductDTO
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? SubCategoryId { get; set; } // Optional for now

        [Required]
        public IFormFile MainImg { get; set; } = null!;
    }
}
