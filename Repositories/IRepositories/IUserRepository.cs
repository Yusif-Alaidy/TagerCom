namespace TagerCom.Repositories.IRepositories
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetUserWithAddressesAsync(string userId);
    }
}
