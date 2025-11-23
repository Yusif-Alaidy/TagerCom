namespace TagerCom.DTOs.Request
{
    public class AddToCartRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
