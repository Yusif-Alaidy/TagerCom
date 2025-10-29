namespace TagerCom.Repositories
{
    public class OrderRepository:Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetVendorSalesAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Order
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.VendorId == vendorId);

            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            return await query.ToListAsync();
        }

    }
}
