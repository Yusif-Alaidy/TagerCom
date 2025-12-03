using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class CreateReview
    {
        public Guid Productid { get; set; }

        [Range(1,5)]
        public int Review { get; set; }
        public string Comment { get; set; }

        public int Rating { get; set; }

    }

}