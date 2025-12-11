using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class CreateReview
    {
        public Guid Productid { get; set; }

        [MaxLength(1000, ErrorMessage="Comment is too long , please make it shorter"),]
        public string Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

    }

}