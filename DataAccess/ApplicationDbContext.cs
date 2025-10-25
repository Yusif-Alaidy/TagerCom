using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using TagerCom.Models;
using UserOTP = TagerCom.Models.UserOTP;


namespace TagerCom.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
         { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserOTP> UserOTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //// تعريف العلاقة بين ApplicationUser و RefreshToken
            //builder.Entity<RefreshToken>()
            //    .HasOne(rt => rt.User)
            //    .WithMany(u => u.RefreshTokens)
            //    .HasForeignKey(rt => rt.UserId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
