namespace TagerCom.Areas.Store.DTOs.Request
{
    public class FilterOrdersRequest
    {
        public DateTime?        startDate               { get; set; }
        public DateTime?        endDate                 { get; set; }
        public OrderStatus?     orderStatus             { get; set; }
        public string           customerUsernameOrEmail { get; set; } = string.Empty;
        public string           OrderNumber             { get; set; } = string.Empty;

        // Sort Option :
        public bool             newest { get; set; }
        public bool             oldest { get; set; }

        // Pagination
        public int pageSize { get; set; } = 10;
        public int page     { get; set; } = 1;
        
    }
}
