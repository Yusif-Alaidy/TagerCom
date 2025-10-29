namespace TagerCom.DTOs.Response
{
    public class VendorSalesResponseDto
    {
        public int VendorId { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalProfit { get; set; }
        public List<TotalOrderSalesDto> Orders { get; set; } = new List<TotalOrderSalesDto>();

    }
}
    