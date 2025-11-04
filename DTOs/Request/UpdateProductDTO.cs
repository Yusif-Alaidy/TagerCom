using System.ComponentModel.DataAnnotations;

namespace TagerCom.DTOs.Request
{
    public class UpdateProductDTO
    {
      

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.1, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        public IFormFile? Image { get; set; } // Optional
    }
}
