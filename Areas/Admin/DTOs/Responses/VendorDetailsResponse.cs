namespace TagerCom.Areas.Admin.DTOs.Responses
{
    public class VendorDetailsResponse
    {
        public VendorDetails        Vendor      { get; set; } = null!;
        public StoreDetails         Store       { get; set; } = null!;
        public VendorPerformance    Performance { get; set; } = null!;
    }
}
