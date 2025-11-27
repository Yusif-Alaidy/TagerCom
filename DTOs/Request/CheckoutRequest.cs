namespace TagerCom.DTOs.Request
{
    public class CheckoutRequest
    {
        public PaymentMethod PaymentMethod { get; set; }
        public int           PointsToUse { get; set; } = 0;  // optional
        public string?       CouponCode { get; set; }


    }
}
