using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection.Emit;
using TagerCom.Models;
using UserOTP = TagerCom.Models.UserOTP;


namespace TagerCom.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
         { }

        public DbSet<RefreshToken>      RefreshTokens   { get; set; }
        public DbSet<UserOTP>           UserOTPs        { get; set; }
        public DbSet<ApplicationUser>   ApplicationUser { get; set; }
        public DbSet<Cart>              Cart            { get; set; }
        public DbSet<CartItem>          CartItem        { get; set; }
        public DbSet<Category>          Category        { get; set; }
        public DbSet<Notification>      Notification    { get; set; }
        public DbSet<Order>             Order           { get; set; }
        public DbSet<OrderItem>         OrderItem       { get; set; }
        public DbSet<Payment>           Payment         { get; set; }
        public DbSet<Product>           Product         { get; set; }
        public DbSet<RefreshToken>      RefreshToken    { get; set; }
        public DbSet<Review>            Review          { get; set; }
        public DbSet<Ticket>            Ticket          { get; set; }
        public DbSet<Transaction>       Transaction     { get; set; }
        public DbSet<UserAddress>       UserAddress     { get; set; }
        public DbSet<UserOTP>           UserOTP         { get; set; }
        public DbSet<Vendor>            Vendor          { get; set; }
        public DbSet<Wallet>            Wallet          { get; set; }
        public DbSet<Favorite> Favorite { get; set; }

        public DbSet<Wishlist> Wishlist { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ConfigureProfile(builder);


            builder.Entity<Order>()
                .HasOne(o => o.Vendor)
                .WithMany()
                .HasForeignKey(o => o.VendorId)
                .OnDelete(DeleteBehavior.Restrict); // 👈 الحل هنا

            builder.Entity<Favorite>().
                HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
   

            builder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Wishlist>()
             .HasOne(w => w.User)
             .WithMany()
             .HasForeignKey(w => w.ApplicationUserId)
             .OnDelete(DeleteBehavior.Cascade);  // حذف المستخدم يمسح كل Wishlists

            builder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.NoAction); // حذف المنتج لا يمسح Wishlists تلقائيًا
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
        private static void ConfigureProfile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProfileImgUrl).HasDefaultValue("default.jpg").ValueGeneratedOnAdd();

            });
        }

    }
}
