namespace TagerCom.Models
{
    public class CartItem
    {
        public Guid     Id              { get; set; } = Guid.NewGuid();
        public Guid     CartId          { get; set; }
        public Guid     ProductId       { get; set; }
        public int      Quantity        { get; set; }
        public decimal  PriceAtAddTime  { get; set; }

        // Navigation
        public Cart     Cart        { get; set; } = null!;
        public Product  Product     { get; set; } = null!;
    }
}
