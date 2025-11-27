using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagerCom.Models
{
    public enum OrderStatus
    {
        // 1. Initial States
        Pending,              // قيد الانتظار (تم إنشاء الطلب)
        AwaitingPayment,      // في انتظار الدفع
        PaymentFailed,        // فشل الدفع

        // 2. After Payment
        Confirmed,            // تم التأكيد (تم الدفع بنجاح)
        Processing,           // قيد المعالجة (البائع يجهز الطلب)

        // 3. Shipping States
        ReadyToShip,          // جاهز للشحن
        Shipped,              // تم الشحن
        OutForDelivery,       // في الطريق للتوصيل

        // 4. Final States
        Delivered,            // تم التسليم
        Completed,            // مكتمل (تم استلامه من العميل)

        // 5. Problem States
        Cancelled,            // ملغي (من العميل أو البائع)
        Refunded,             // تم الاسترجاع
        Returned,             // تم الإرجاع
        Failed                // فشل الطلب
    }
    public class Order
    {
        public Guid          Id                  { get; set; } = Guid.NewGuid();
        public string        ApplicationUserId   { get; set; } = string.Empty;
        public Guid          StoreId             { get; set; }
        [EnumDataType(typeof(StoreStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus   OrderStatus         { get; set; } = OrderStatus.Pending;
        public decimal?      TotalAmount         { get; set; }
        public DateTime      CreatedAt           { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<OrderItem>           OrderItems      { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory>  StatusHistory   { get; set; } = new List<OrderStatusHistory>();
        public ApplicationUser                  ApplicationUser { get; set; } = null!;
        public Store                            Store           { get; set; } = null!;
        public Payment?                         Payment         { get; set; }
    }


}
