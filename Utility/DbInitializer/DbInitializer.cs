using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.IdentityModel.Tokens;
using TagerCom.DataAccess;
using TagerCom.Utility.DbInitalizer;

namespace TagerCom.Utility.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        #region Fields
        public ApplicationDbContext Context { get; }
        public UserManager<ApplicationUser> UserManger { get; }
        public RoleManager<IdentityRole> RoleManger { get; }
        private readonly ILogger<DbInitializer> _logger;
        private readonly IRepository<Category> repoCategory;
        private readonly IRepository<Brand> repoBrand;
        private readonly IRepository<Product> repoProduct;
        private readonly IRepository<Store> repoStore;
        private readonly IRepository<Cart> repoCart;
        private readonly IRepository<CartItem> repoCartItem;
        private readonly IRepository<Order> repoOrder;
        private readonly IRepository<OrderItem> repoOrderItem;

        #endregion

        #region Constructore
        public DbInitializer(ApplicationDbContext Context, UserManager<ApplicationUser> UserManger, RoleManager<IdentityRole> RoleManger, ILogger<DbInitializer> logger, IRepository<Category> RepoCategory, IRepository<Brand> RepoBrand, IRepository<Product> RepoProduct, IRepository<Store> RepoStore, IRepository<Cart> RepoCart, IRepository<CartItem> RepoCartItem, IRepository<Order> RepoOrder, IRepository<OrderItem> RepoOrderItem)
        {
            this.Context        = Context;
            this.UserManger     = UserManger;
            this.RoleManger     = RoleManger;
            this._logger        = logger;
            this.repoCategory   = RepoCategory;
            this.repoBrand      = RepoBrand;
            this.repoProduct    = RepoProduct;
            this.repoStore      = RepoStore;
            this.repoCart = RepoCart;
            this.repoCartItem = RepoCartItem;
            this.repoOrder = RepoOrder;
            this.repoOrderItem = RepoOrderItem;
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
                if (!RoleManger.Roles.Any())
                {

                    RoleManger.CreateAsync(new("Admin")).GetAwaiter().GetResult();
                    RoleManger.CreateAsync(new("Vendor")).GetAwaiter().GetResult();
                    RoleManger.CreateAsync(new("Customer")).GetAwaiter().GetResult();
                    RoleManger.CreateAsync(new("CustomerService")).GetAwaiter().GetResult();



                    var result = UserManger.CreateAsync(new()
                    {
                        UserName = "Admin",
                        Email = "Admin@gmail.com",
                        EmailConfirmed = true,

                    }, "Admin@123").GetAwaiter().GetResult();

                    var user = UserManger.FindByEmailAsync("Admin@gmail.com").GetAwaiter().GetResult();
                    user!.EmailConfirmed = true;

                    if (user is not null)
                        UserManger.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();

                    // Seed Category ====================================================================
                    repoCategory.AddAsync(new Category { Name = "Electronics" }).GetAwaiter().GetResult();
                    repoCategory.CommitAsync().GetAwaiter().GetResult();

                    var Electronics = repoCategory.GetOneAsync(category => category.Name == "Electronics").GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "Laptops",          Parent = Electronics }).GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "Mobiles",          Parent = Electronics }).GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "Cameras",          Parent = Electronics }).GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "Tablets",          Parent = Electronics }).GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "Smart Watches",    Parent = Electronics }).GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "TV & Display",     Parent = Electronics }).GetAwaiter().GetResult();
                    repoCategory.CommitAsync().GetAwaiter().GetResult();


                    var Mobiles = repoCategory.GetOneAsync(category => category.Name == "Mobiles").GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "Mobile Accessories", Parent = Mobiles }).GetAwaiter().GetResult();
                    repoCategory.CommitAsync().GetAwaiter().GetResult();

                    var Laptops = repoCategory.GetOneAsync(category => category.Name == "Laptops").GetAwaiter().GetResult();
                    repoCategory.AddAsync(new Category { Name = "Laptop Accessories", Parent = Laptops }).GetAwaiter().GetResult();
                    repoCategory.CommitAsync().GetAwaiter().GetResult();

                    var Camera              = repoCategory.GetOneAsync(e=>e.Name == "Cameras").GetAwaiter().GetResult();
                    var Tablets             = repoCategory.GetOneAsync(e=>e.Name == "Tablets").GetAwaiter().GetResult();
                    var Smart_Watches       = repoCategory.GetOneAsync(e=>e.Name == "Smart Watches").GetAwaiter().GetResult();
                    var TV_Display          = repoCategory.GetOneAsync(e=>e.Name == "TV & Display").GetAwaiter().GetResult();
                    var Laptop__accessories = repoCategory.GetOneAsync(e=>e.Name == "Laptop Accessories").GetAwaiter().GetResult();
                    var Mobile__accessories = repoCategory.GetOneAsync(e=>e.Name == "Mobile Accessories").GetAwaiter().GetResult();
                    // ==================================================================================

                    // Seed Data For All Table ==========================================================

                    // Seed Brand
                    repoBrand.AddAsync(new Brand { BrandName = "Hp"}).GetAwaiter().GetResult();
                    repoBrand.AddAsync(new Brand { BrandName = "Samsung"}).GetAwaiter().GetResult();
                    repoBrand.AddAsync(new Brand { BrandName = "Apple"}).GetAwaiter().GetResult();
                    repoBrand.AddAsync(new Brand { BrandName = "Nikon"}).GetAwaiter().GetResult();
                    repoBrand.AddAsync(new Brand { BrandName = "Cannon"}).GetAwaiter().GetResult();
                    repoBrand.AddAsync(new Brand { BrandName = "Oppo"}).GetAwaiter().GetResult();
                    repoBrand.AddAsync(new Brand { BrandName = "Sony"}).GetAwaiter().GetResult();
                    repoBrand.CommitAsync().GetAwaiter().GetResult();
                    var Hp      = repoBrand.GetOneAsync(e=>e.BrandName == "HP").GetAwaiter().GetResult();
                    var Samsung = repoBrand.GetOneAsync(e=>e.BrandName == "Samsung").GetAwaiter().GetResult();
                    var Apple   = repoBrand.GetOneAsync(e=>e.BrandName == "Apple").GetAwaiter().GetResult();
                    var Cannon  = repoBrand.GetOneAsync(e=>e.BrandName == "Cannon").GetAwaiter().GetResult();
                    var Oppo    = repoBrand.GetOneAsync(e=>e.BrandName == "Oppo").GetAwaiter().GetResult();
                    var Sony    = repoBrand.GetOneAsync(e=>e.BrandName == "Sony").GetAwaiter().GetResult();
                    // Seed Users
                    var user1 = UserManger.CreateAsync(new()
                    {
                        UserName = "User",
                        Email = "User@gmail.com",
                        EmailConfirmed = true,

                    }, "Customer@123").GetAwaiter().GetResult();
                    var user1_result = UserManger.FindByEmailAsync("User@gmail.com").GetAwaiter().GetResult();
                    if (user1_result is not null)
                        UserManger.AddToRoleAsync(user1_result, "Customer").GetAwaiter().GetResult();

                    var user2 = UserManger.CreateAsync(new()
                    {
                        UserName = "User2",
                        Email = "User2@gmail.com",
                        EmailConfirmed = true,

                    }, "Customer@123").GetAwaiter().GetResult();
                    var user2_result = UserManger.FindByEmailAsync("User2@gmail.com").GetAwaiter().GetResult();
                    if (user2_result is not null)
                        UserManger.AddToRoleAsync(user2_result, "Customer").GetAwaiter().GetResult();

                    var user3 = UserManger.CreateAsync(new()
                    {
                        UserName = "User3",
                        Email = "User3@gmail.com",
                        EmailConfirmed = true,

                    }, "Customer@123").GetAwaiter().GetResult();
                    var user3_result = UserManger.FindByEmailAsync("User3@gmail.com").GetAwaiter().GetResult();
                    if (user3_result is not null)
                        UserManger.AddToRoleAsync(user3_result, "Customer").GetAwaiter().GetResult();

                    // Seed Vendors
                    var vendor1 = UserManger.CreateAsync(new()
                    {
                        UserName = "Vendor",
                        Email = "Vendor@gmail.com",
                        EmailConfirmed = true,

                    }, "Vendor@123").GetAwaiter().GetResult();
                    var vendor1_result = UserManger.FindByEmailAsync("Vendor@gmail.com").GetAwaiter().GetResult();
                    if (vendor1_result is not null)
                        UserManger.AddToRoleAsync(vendor1_result, "Vendor").GetAwaiter().GetResult();


                    var vendor2 = UserManger.CreateAsync(new()
                    {
                        UserName = "Vendor2",
                        Email = "Vendor2@gmail.com",
                        EmailConfirmed = true,

                    }, "Vendor@123").GetAwaiter().GetResult();
                    var vendor2_result = UserManger.FindByEmailAsync("Vendor2@gmail.com").GetAwaiter().GetResult();
                    if (vendor2_result is not null)
                        UserManger.AddToRoleAsync(vendor2_result, "Vendor").GetAwaiter().GetResult();


                    var vendor3 = UserManger.CreateAsync(new()
                    {
                        UserName = "Vendor3",
                        Email = "Vendor3@gmail.com",
                        EmailConfirmed = true,

                    }, "Vendor@123").GetAwaiter().GetResult();
                    var vendor3_result = UserManger.FindByEmailAsync("Vendor3@gmail.com").GetAwaiter().GetResult();
                    if (vendor3_result is not null)
                        UserManger.AddToRoleAsync(vendor3_result, "Vendor").GetAwaiter().GetResult();

                    // Seed Stores
                    repoStore.AddAsync(new Store { ApplicationUserId = vendor1_result!.Id, StoreName = "Amazon_store", IsActive = true, Status = StoreStatus.Approved }).GetAwaiter().GetResult();
                    repoStore.AddAsync(new Store { ApplicationUserId = vendor2_result!.Id, StoreName = "Yusif_store",  IsActive = true, Status = StoreStatus.Approved }).GetAwaiter().GetResult();
                    repoStore.AddAsync(new Store { ApplicationUserId = vendor3_result!.Id, StoreName = "Noon_store",   IsActive = true, Status = StoreStatus.Approved }).GetAwaiter().GetResult();
                    repoStore.CommitAsync().GetAwaiter().GetResult(); 
                    var store  = repoStore.GetOneAsync(e => e.StoreName == "Amazon_store").GetAwaiter().GetResult();
                    var store2 = repoStore.GetOneAsync(e => e.StoreName == "Yusif_store").GetAwaiter().GetResult();
                    var store3 = repoStore.GetOneAsync(e => e.StoreName == "Noon_store").GetAwaiter().GetResult();

                    // Seed Product
                    repoProduct.AddAsync(new Product { Name = "Iphone 16",            StoreId = store!.Id, CategoryId = Mobiles!.Id,             BrandId = Apple!.Id,   Description = "This Description", Price = 16000.40m, Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "Laptop Hp Victus 15",  StoreId = store.Id,  CategoryId = Laptops!.Id,             BrandId = Hp!.Id,      Description = "This Description", Price = 32000.40m, Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "iPhone Charger",       StoreId = store.Id,  CategoryId = Laptop__accessories!.Id, BrandId = Apple.Id,    Description = "This Description", Price = 1000.40m,  Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "Laptop Charger",       StoreId = store.Id,  CategoryId = Mobile__accessories!.Id, BrandId = Hp.Id,       Description = "This Description", Price = 1800.40m,  Stock = 50 }).GetAwaiter().GetResult();
                    
                    repoProduct.AddAsync(new Product { Name = "Iphone 11",            StoreId = store2!.Id, CategoryId = Mobiles.Id,             BrandId = Apple.Id,    Description = "This Description", Price = 9000.40m,  Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "samsung a11",          StoreId = store2.Id,  CategoryId = Mobiles.Id,             BrandId = Samsung!.Id, Description = "This Description", Price = 7000.40m,  Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "canone m50",           StoreId = store2.Id,  CategoryId = Camera!.Id,             BrandId = Cannon!.Id,  Description = "This Description", Price = 35000.40m, Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "samsung tv",           StoreId = store2.Id,  CategoryId = TV_Display!.Id,         BrandId = Samsung.Id,  Description = "This Description", Price = 61000.40m, Stock = 50 }).GetAwaiter().GetResult();

                    repoProduct.AddAsync(new Product { Name = "Oppo F12",             StoreId = store3!.Id, CategoryId = Mobiles.Id,             BrandId = Oppo!.Id,    Description = "This Description", Price = 18000.40m, Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "Samsung Taplet",       StoreId = store3.Id,  CategoryId = Tablets!.Id,            BrandId = Samsung.Id,  Description = "This Description", Price = 23000.40m, Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "Ipad pro 6",           StoreId = store3.Id,  CategoryId = Tablets.Id,             BrandId = Apple.Id,    Description = "This Description", Price = 15000.40m, Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.AddAsync(new Product { Name = "Iphone 16",            StoreId = store3.Id,  CategoryId = Mobiles.Id,             BrandId = Apple.Id,    Description = "This Description", Price = 83000.40m, Stock = 50 }).GetAwaiter().GetResult();
                    repoProduct.CommitAsync().GetAwaiter().GetResult();

                    var product1 = repoProduct.GetOneAsync(e=>e.Name == "Iphone 16").GetAwaiter().GetResult();
                    var product2 = repoProduct.GetOneAsync(e=>e.Name == "Laptop Hp Victus 15").GetAwaiter().GetResult();
                    var product3 = repoProduct.GetOneAsync(e=>e.Name == "iPhone Charger").GetAwaiter().GetResult();
                    var product4 = repoProduct.GetOneAsync(e=>e.Name == "samsung a11").GetAwaiter().GetResult();
                    var product5 = repoProduct.GetOneAsync(e=>e.Name == "Oppo F12").GetAwaiter().GetResult();

                    // Seed Carts
                    repoCart.AddAsync(new Cart { UserId = user1_result!.Id}).GetAwaiter().GetResult();
                    repoCart.CommitAsync().GetAwaiter().GetResult();
                    var cart1 = repoCart.GetOneAsync(e => e.UserId == user1_result.Id).GetAwaiter().GetResult();
                    repoCartItem.AddAsync(new CartItem { CartId = cart1!.Id, ProductId = product1!.Id, PriceAtAddTime = product1.Price, Quantity = 5} );
                    repoCartItem.AddAsync(new CartItem { CartId = cart1.Id, ProductId = product4!.Id, PriceAtAddTime = product4.Price, Quantity = 5} );

                    repoCart.AddAsync(new Cart { UserId = user2_result!.Id }).GetAwaiter().GetResult();
                    repoCart.CommitAsync().GetAwaiter().GetResult();
                    var cart2 = repoCart.GetOneAsync(e => e.UserId == user2_result.Id).GetAwaiter().GetResult();
                    repoCartItem.AddAsync(new CartItem { CartId = cart2!.Id, ProductId = product2!.Id, PriceAtAddTime = product2.Price, Quantity = 5 });
                    repoCartItem.AddAsync(new CartItem { CartId = cart2.Id,  ProductId = product5!.Id, PriceAtAddTime = product5.Price, Quantity = 5 });
                    repoCartItem.CommitAsync().GetAwaiter().GetResult();

                    // Seed Order
                    repoOrder.AddAsync(new Order { ApplicationUserId = user1_result.Id, StoreId = store.Id, }).GetAwaiter().GetResult();
                    repoOrderItem.CommitAsync().GetAwaiter().GetResult();

                    var order1 = repoOrder.GetOneAsync(e => e.ApplicationUserId == user1_result.Id).GetAwaiter().GetResult();
                    repoOrderItem.AddAsync(new OrderItem { OrderId = order1!.Id, ProductId = product1.Id, Quantity = 3, Price = product1.Price});
                    repoOrderItem.AddAsync(new OrderItem { OrderId = order1.Id, ProductId = product2.Id, Quantity = 3, Price = product2.Price});
                    repoOrderItem.AddAsync(new OrderItem { OrderId = order1.Id, ProductId = product3!.Id, Quantity = 3, Price = product3.Price});
                    repoOrderItem.CommitAsync().GetAwaiter().GetResult();

                    repoOrder.AddAsync(new Order { ApplicationUserId = user2_result.Id, StoreId = store2.Id, }).GetAwaiter().GetResult();
                    repoOrderItem.CommitAsync().GetAwaiter().GetResult();

                    var order2 = repoOrder.GetOneAsync(e => e.ApplicationUserId == user2_result.Id).GetAwaiter().GetResult();
                    repoOrderItem.AddAsync(new OrderItem { OrderId = order2!.Id, ProductId = product1.Id, Quantity = 3, Price = product1.Price });
                    repoOrderItem.AddAsync(new OrderItem { OrderId = order2.Id, ProductId = product2.Id, Quantity = 3, Price = product2.Price });
                    repoOrderItem.AddAsync(new OrderItem { OrderId = order2.Id, ProductId = product3.Id, Quantity = 3, Price = product3.Price });
                    repoOrderItem.CommitAsync().GetAwaiter().GetResult();


                    // ==================================================================================
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
