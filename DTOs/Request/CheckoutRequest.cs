namespace TagerCom.DTOs.Request
{
    public class CheckoutRequest
    {
        public string PaymentMethod { get; set; } = "Cash";  // "Cash" or "Online"
        public int PointsToUse { get; set; } = 0;  // optional
        public string? CouponCode { get; set; }


    }
}
