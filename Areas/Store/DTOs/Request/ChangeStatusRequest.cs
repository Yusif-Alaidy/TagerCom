using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Store.DTOs.Request
{
    public class ChangeStatusRequest
    {
        [Required]
        public OrderStatus? orderStatus { get; set; }
    }
}
