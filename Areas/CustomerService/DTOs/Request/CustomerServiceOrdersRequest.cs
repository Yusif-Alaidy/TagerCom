namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class CustomerServiceOrdersRequest
    {
        public string? OrderNumber { get; set; }

        // Filters ------------------------------------------
        public Guid? VendorId { get; set; }              

        public string? CustomerUsername { get; set; }
        public string? CustomerEmail { get; set; }

        public OrderStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // sorting ------------------------------------------
        // date - amount - status
        public string? SortBy { get; set; }
        public bool? Descending { get; set; } = true;
        // --------------------------------------------------

        // Pagination ---------------------------------------
        public int CurrentPage { get; set; } = 1;
    }
}
