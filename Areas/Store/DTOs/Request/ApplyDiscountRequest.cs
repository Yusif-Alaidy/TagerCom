using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Store.DTOs.Request
{
    public class ApplyDiscountRequest
    {
        [Required]
        public decimal? DiscountValueFixed { get; set; }
        [Required]
        public DateTime? DiscountStartDate { get; set; }
        [Required]
        public DateTime? DiscountEndDate { get; set; }
    }
}
