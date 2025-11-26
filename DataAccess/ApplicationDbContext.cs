using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

        public DbSet<Brand>             Brands              { get; set; }
        public DbSet<RefreshToken>      RefreshTokens       { get; set; }
        public DbSet<UserOTP>           UserOTPs            { get; set; }
        public DbSet<ApplicationUser>   ApplicationUsers    { get; set; }
        public DbSet<Cart>              Carts               { get; set; }
        public DbSet<CartItem>          CartItems           { get; set; }
        public DbSet<Category>          Categories          { get; set; }
        public DbSet<Notification>      Notifications       { get; set; }
        public DbSet<Order>             Orders              { get; set; }
        public DbSet<OrderItem>         OrderItems          { get; set; }
        public DbSet<Payment>           Payments            { get; set; }
        public DbSet<Product>           Products            { get; set; }
        public DbSet<Review>            Reviews             { get; set; }
        public DbSet<Ticket>            Tickets             { get; set; }
        public DbSet<Transaction>       Transactions        { get; set; }
        public DbSet<UserAddress>       UserAddress         { get; set; }
        public DbSet<Store>             Stores              { get; set; }
        public DbSet<Wallet>            Wallets             { get; set; }

        public DbSet<Wishlist> Wishlist { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ConfigureProfile(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var idProperty = entityType.FindProperty("Id");

                if (idProperty != null)
                {
                    
                    if (idProperty.ClrType == typeof(Guid))
                    {
                        idProperty.ValueGenerated = ValueGenerated.OnAdd;
                    }

                    
                    if (idProperty.ClrType == typeof(int))
                    {
                        idProperty.ValueGenerated = ValueGenerated.OnAdd;
                    }
                }
            }

            builder.Entity<Ticket>()
                .HasOne(t => t.Support)
                .WithMany()
                .HasForeignKey(t => t.SupportId);

            
            builder.Entity<Product>()
                .HasIndex(e => e.StoreId);

            builder.Entity<Product>()
                .HasIndex(e => e.CategoryId);

            builder.Entity<Order>()
                .HasIndex(e => e.ApplicationUserId);

            builder.Entity<Order>()
                .HasIndex(e => e.StoreId);

            builder.Entity<OrderItem>()
                .HasIndex(e => e.ProductId);

            builder.Entity<Review>()
                .HasIndex(e => e.ProductId);

            builder.Entity<Review>()
                .HasIndex(e => e.CustomerId);

            builder.Entity<Cart>()
                .HasIndex(e => e.UserId);

            builder.Entity<CartItem>()
                .HasIndex(e => e.ProductId);

            builder.Entity<Ticket>()
                .HasIndex(e => e.SupportId);


            // Referential Action
            // ============================================
            // ApplicationUser → UserAddress (One-to-Many)
            // ============================================
            builder.Entity<UserAddress>()
                .HasOne(ua => ua.ApplicationUser)
                .WithMany(u => u.userAddresses)
                .HasForeignKey(ua => ua.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // ============================================
            // ApplicationUser → UserAddress (One-to-Many)
            // ============================================
            builder.Entity<Category>()
                .HasOne(ua => ua.Parent)
                .WithMany(u => u.Chiled)
                .HasForeignKey(ua => ua.ParentId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // Brand → Product (One-to-Many)
            // ============================================
            builder.Entity<Product>()
                .HasOne(ua => ua.Brand)
                .WithMany(u => u.Products)
                .HasForeignKey(ua => ua.BrandId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // ApplicationUser → Review (One-to-Many)
            // ============================================
            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // ApplicationUser → Cart (One-to-One)
            // ============================================
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // ApplicationUser → Order (One-to-Many)
            // ============================================
            builder.Entity<Order>()
                .HasOne(o => o.ApplicationUser)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // ApplicationUser → Vendor (One-to-One)
            // ============================================
            builder.Entity<Store>()
                .HasOne(v => v.ApplicationUser)
                .WithOne(u => u.Store)
                .HasForeignKey<Store>(v => v.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // Vendor → Product (One-to-Many)
            // ============================================
            builder.Entity<Product>()
                .HasOne(p => p.Store)
                .WithMany(v => v.Products)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // Vendor → Order (One-to-Many)
            // ============================================
            builder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // Product → Review (One-to-Many)
            // ============================================
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // Product → CartItem (One-to-Many)
            // ============================================
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // Category → Product (One-to-Many)
            // ============================================
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // Cart → CartItem (One-to-Many)
            // ============================================
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // Order → OrderItem (One-to-Many)
            // ============================================
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // Product → OrderItem (One-to-Many)
            // ============================================
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
      
            // Validation =================================
            // Ensure unique cart per user
            builder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Ensure unique vendor per user
            builder.Entity<Store>()
                .HasIndex(v => v.ApplicationUserId)
                .IsUnique();

            // Set decimal precision for financial fields
            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<CartItem>()
                .Property(ci => ci.PriceAtAddTime)
                .HasPrecision(18, 2);

            builder.Entity<Store>()
                .Property(v => v.Rating)
                .HasPrecision(3, 2);

            builder.Entity<Store>()
                .Property(v => v.RevenueShare)
                .HasPrecision(5, 4);
            // ============================================
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
