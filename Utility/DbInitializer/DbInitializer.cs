using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TagerCom.DataAccess;
using TagerCom.Utility.DbInitializer;

namespace TagerCom.Utility.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        #region Fields
        public ApplicationDbContext Context { get; }
        public UserManager<ApplicationUser> UserManger { get; }
        public RoleManager<IdentityRole> RoleManger { get; }
        private readonly ILogger<DbInitializer> _logger;

        #endregion

        #region Constructore
        public DbInitializer(ApplicationDbContext Context, UserManager<ApplicationUser> UserManger, RoleManager<IdentityRole> RoleManger, ILogger<DbInitializer> logger)
        {
            this.Context =Context;
            this.UserManger = UserManger;
            this.RoleManger = RoleManger;
            this._logger = logger;

        }
        #endregion
        
        #region Initialize
        public void Initialize()
        {
            try
            {
                // Create Update-database --------------------------------
                var pending = Context.Database.GetPendingMigrations();
                if (pending.Any())
                {
                    Console.WriteLine("Applying pending migrations...");
                    Context.Database.Migrate();
                    Console.WriteLine("Migrations applied successfully.");
                }



                // -------------------------------------------------------

                // Seed data for role --------------------------------------------------------
                if (!RoleManger.Roles.Any())

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
