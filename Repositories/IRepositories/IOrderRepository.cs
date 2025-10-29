namespace TagerCom.Repositories.IRepositories
{
    public interface IOrderRepository: IRepository<Order>

    {
        Task<List<Order>> GetVendorSalesAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null);

    }
}
