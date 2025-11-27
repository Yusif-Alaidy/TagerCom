namespace TagerCom.DTOs.Request
{
    public class AddToCartRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
