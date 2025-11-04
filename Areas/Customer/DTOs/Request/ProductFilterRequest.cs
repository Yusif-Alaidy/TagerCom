namespace TagerCom.Areas.Customer.DTOs.Request
{
    public class ProductFilterRequest
    {
        // Search by name and description -------------------
        public string? Search {get; set;}
        // --------------------------------------------------

        // Filter -------------------------------------------
        public decimal? MinPrice {get; set;}
        public decimal? MaxPrice {get; set;}
        public String? Category {get; set;}
        // --------------------------------------------------

        // sorting ------------------------------------------
        public string? SortBy {get; set;} // Price - Date - Rate
        public bool? OrderByDescending {get; set;}
        // --------------------------------------------------

        // Paggination --------------------------------------
        public int Page { get; set; } = 1;
        // --------------------------------------------------
    }
}
