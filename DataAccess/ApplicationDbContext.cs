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
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<UserAddress> UserAddress { get; set; }
        public DbSet<UserOTP> UserOTP { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<Wallet> Wallet { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // تعريف العلاقة بين ApplicationUser و RefreshToken
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);
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
