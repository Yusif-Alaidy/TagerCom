namespace TagerCom.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserWithAddressesAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.userAddresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
