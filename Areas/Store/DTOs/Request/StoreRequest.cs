using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Areas.Store.DTOs.Request
{
    public class StoreRequest
    {
        [Required]
        public string StoreName { get; set; } = string.Empty;
    }
}
