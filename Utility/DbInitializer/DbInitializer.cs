using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TagerCom.DataAcess;
using TagerCom.Utility.DbInitalizer;

namespace TagerCom.Utility.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        #region Fields
        public ApplicationDbContext Context { get; }
        public UserManager<IdentityUser> UserManger { get; }
        public RoleManager<IdentityRole> RoleManger { get; }
        private readonly ILogger<DbInitializer> _logger;

        #endregion

        #region Constructore
        public DbInitializer(ApplicationDbContext Context, UserManager<IdentityUser> UserManger, RoleManager<IdentityRole> RoleManger, ILogger<DbInitializer> logger)
        {
            this.Context =Context;
            this.UserManger = UserManger;
            this.RoleManger = RoleManger;
            this._logger = _logger;

        }
        #endregion

        #region Initialize
        public void Initialize()
        {
            try
            {
                // Create Update-database --------------------------------
                if (Context.Database.GetPendingMigrations().Any())
                {
                    Context.Database.Migrate();
                }
                // -------------------------------------------------------

                // Seed data for role --------------------------------------------------------
                if (RoleManger.Roles.IsNullOrEmpty())
                {

                    RoleManger.CreateAsync(new("SuperAdmin")).GetAwaiter().GetResult();
                    RoleManger.CreateAsync(new("Admin")).GetAwaiter().GetResult();
                    RoleManger.CreateAsync(new("Vendor")).GetAwaiter().GetResult();
                    RoleManger.CreateAsync(new("Customer")).GetAwaiter().GetResult();
                    RoleManger.CreateAsync(new("CustomerService")).GetAwaiter().GetResult();



                    var result = UserManger.CreateAsync(new()
                    {
                        UserName = "SuperAdmin",
                        Email = "SuperAdmin@gmail.com",
                        EmailConfirmed = true,

                    }, "Admin@123").GetAwaiter().GetResult();
                    var user = UserManger.FindByEmailAsync("SuperAdmin@gmail.com").GetAwaiter().GetResult();
                    user.EmailConfirmed = true;
                    if (user is not null)
                        UserManger.AddToRoleAsync(user, "SuperAdmin").GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError("Check connection. Use DB on local server (.)");
            }
            // ---------------------------------------------------------------------------
        }
        #endregion
    }
}
