namespace TagerCom.Areas.Admin.DTOs.Responses
{
    public class VendorListItemResponse
    {
        public string VendorId { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string VendorEmail { get; set; } = string.Empty;

        public Guid? StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;

        public StoreStatus? Status { get; set; }
        public bool? IsActive { get; set; }

        public DateTime? RegisteredAt { get; set; }

        // KPIs
        public int TotalSales { get; set; }           // total quantity sold (all time)
        public decimal TotalRevenue { get; set; }     // total revenue (all time)
    }

    public class PagedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    }
}
