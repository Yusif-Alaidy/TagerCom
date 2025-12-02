namespace TagerCom.Areas.Customer.DTOs.Response
{
    public class VisitStoreResponse
    {
        public String StoreName { get; set; } = string.Empty;
        public decimal Rating { get; set; }

        public List<ProductsRespons> products { get; set; } = new();
        
    }
}
