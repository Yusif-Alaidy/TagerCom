using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.DTOs.Request
{
    public class UpdateVendorStatusRequest
    {
        [Required]
        public Guid? VendorId { get; set; }
        [Required]
        [EnumDataType(typeof(StoreStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StoreStatus? ApprovedOrRejected { get; set; }
    }
}
