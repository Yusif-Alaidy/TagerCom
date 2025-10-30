using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.DTOs.Request
{
    public class UpdateVendorStatusRequest
    {
        [Required]
        public Guid? VendorId { get; set; }
        [Required]
        [EnumDataType(typeof(VendorStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VendorStatus? ApprovedOrRejected { get; set; }
    }
}
