using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Models
{
    public enum OrderStatus
    {
        // 1. Initial States
        Pending,              // 0
        AwaitingPayment,      // 1
        PaymentFailed,        // 2

        // 2. After Payment
        Confirmed,            // 3
        Processing,           // 4

        // 3. Shipping States
        ReadyToShip,          // 5
        Shipped,              // 6
        OutForDelivery,       // 7

        // 4. Final States
        Delivered,            // 8
        Completed,            // 9

        // 5. Problem States
        Cancelled,            // 10 
        Refunded,             // 11
        Returned,             // 12
        Failed                // 13
    }
    public class Order
    {
        public Guid           Id                 { get; set; } = Guid.NewGuid();
        public string?        CustomerId         { get; set; } = string.Empty;
        public Guid?          StoreId            { get; set; }
        [EnumDataType(typeof(StoreStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus   OrderStatus         { get; set; } = OrderStatus.Pending;
        public decimal?      TotalAmount         { get; set; }
        public DateTime      CreatedAt           { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<OrderItem>           OrderItems      { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory>  StatusHistory   { get; set; } = new List<OrderStatusHistory>();
        public ApplicationUser                  Customer        { get; set; } = null!;
        public Store?                           Store           { get; set; } = null!;
        public Payment?                         Payment         { get; set; }
    }


}
