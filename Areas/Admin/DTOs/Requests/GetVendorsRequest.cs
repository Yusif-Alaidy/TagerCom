using System.ComponentModel.DataAnnotations;

namespace TagerCom.Areas.Admin.DTOs.Requests
{
    public enum VendorSortBy
    {
        Date = 0,
        Sales = 1,
        Revenue = 2
    }

    public class GetVendorsRequest
    {
        // Pagination
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        // Search (Name, Email, Store Name)
        public string? Search { get; set; }

        // Filters
        public StoreStatus? Status { get; set; }          // Pending, Approved, Rejected, Suspended
        public bool? IsActive { get; set; }               // true/false
        public DateTime? StartDate { get; set; }          // Registration date range
        public DateTime? EndDate { get; set; }

        // Sorting
        public VendorSortBy SortBy { get; set; } = VendorSortBy.Date;
        public bool Desc { get; set; } = true;
    }
}
