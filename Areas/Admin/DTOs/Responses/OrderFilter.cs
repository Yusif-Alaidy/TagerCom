namespace TagerCom.Areas.Admin.DTOs.Responses
{
    public class OrderFilter
    {
        public OrderStatus? orderStatus  { get; set; }
        public string CustomerId        { get; set; } = string.Empty;
        public Guid? StoreID             { get; set; }
        public DateTime? startDate       { get; set; }
        public DateTime? endDate         { get; set; }

        // Pagination
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 50;

    }
}
