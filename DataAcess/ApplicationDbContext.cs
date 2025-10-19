using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TagerCom.DataAcess
{
    public class ApplicationDbContext : IdentityDbContext
    {
        #region Constructor

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        #endregion


    }
}
