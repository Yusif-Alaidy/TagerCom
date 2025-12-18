using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Areas.Store.DTOs.Response
{
    public class StoreResponse
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string StoreName { get; set; } = null!;
        public decimal Rating { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
    }
}
