using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TagerCom.Models
{
    public class OrderItem
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public Guid     OrderId     { get; set; }
        public Guid     ProductId   { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity         { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price        { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order    Order       { get; set; } = null!;
        public Product  Product     { get; set; } = null!;



    }
}
