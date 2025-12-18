using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TagerCom.Areas.Store.DTOs.Response;
using TagerCom.Areas.Store.DTOs.Request;

namespace TagerCom.Areas.Store.Controllers
{
    [Area("Store")]
    [Route("api/store/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendor")]
    public class ProfileController : ControllerBase
    {
        #region Fields
        public UserManager<ApplicationUser> UserManager { get; }
        public IRepository<Models.Store> StoreRepo { get; }
        public IRepository<Order> OrderRepo { get; }
        public IRepository<Product> ProductRepo { get; }
        public IRepository<CartItem> CartItemRepo { get; }
        #endregion

        #region Constructore
        public ProfileController(UserManager<ApplicationUser> UserManager, IRepository<Models.Store> StoreRepo, IRepository<Order> OrderRepo, IRepository<Product> ProductRepo, IRepository<CartItem> CartItemRepo)
        {
            this.UserManager  = UserManager;
            this.StoreRepo    = StoreRepo;
            this.OrderRepo    = OrderRepo;
            this.ProductRepo  = ProductRepo;
            this.CartItemRepo = CartItemRepo;
        }

        #endregion

        #region view store
        [HttpGet]
        public async Task<IActionResult> GetStore()
        {
            // Get user =========================================================================================================
            // ==================================================================================================================
            var user = await UserManager.GetUserAsync(User);
            // ==================================================================================================================


            // Get Store ========================================================================================================
            // ==================================================================================================================

            var store = (await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive));
            if (store is null)
            {
                return BadRequest(new { message = "This store is not exist!" });
            }
            var response = new StoreResponse
            {
                Id              = store!.Id,
                StoreName       = store.StoreName,
                Rating          = store.Rating,
                CreatedAt       = store.CreatedAt,
                UpdatedAt       = store.UpdatedAt,
            };
            // ==================================================================================================================

            return Ok(response);
            
        }
        #endregion

        #region Update store
        [HttpPut]
        public async Task<IActionResult> UpdateStore(StoreRequest request)
        {
            // Get user ==============================
            var user = await UserManager.GetUserAsync(User);
            // =======================================

            // Get Store =============================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive);
            if (store is null)
            {
                return BadRequest(new { message = "This store is not exist!" });
            }
            // =======================================

            // Update Store ==========================
            store.StoreName = request.StoreName;
            store.UpdatedAt = DateTime.UtcNow;
            await StoreRepo.CommitAsync();
            // =======================================
            return Ok(new {message = "Update Successfully!"});
        }
        #endregion

        #region Delete store
        [HttpDelete]
        public async Task<IActionResult> DeleteStore()
        {
            // Get user ===========================================================================================================================
            // ====================================================================================================================================
            var user = await UserManager.GetUserAsync(User);
            // ====================================================================================================================================

            // Get store ==========================================================================================================================
            // ====================================================================================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive, includes:[equals=>equals.Orders]);
            if (store is null)
            {
                return BadRequest(new { message = "This store is not exist!" });
            }
            // ===================================================================================================================================

            // Chech if can delete ===============================================================================================================
            // ===================================================================================================================================
            var hasActiveOrder = store.Orders.Any(e=>
            e.OrderStatus != OrderStatus.Refunded  &&
            e.OrderStatus != OrderStatus.Cancelled &&
            e.OrderStatus != OrderStatus.Completed );
            if (hasActiveOrder)
            {
                return BadRequest(new
                {
                    message = "Cannot delete account. You have active orders. Please complete or cancel them first."
                });
            }
            // ===================================================================================================================================


            // Delete Proccess ===================================================================================================================
            // ===================================================================================================================================
            store.IsDeleted = true;
            store.IsActive  = false;

            var products = await ProductRepo.GetAsync(equals=>equals.StoreId == store.Id && !equals.IsDeleted);
            var productsIds = products.Select(e => e.Id);
            // Handle Cart Item
            //var cartsItem = (await CartItemRepo.GetAsync(e=>e.Product.StoreId == store.Id ,includes:[equals=>equals.Product])).AsQueryable().ExecuteDeleteAsync();  
            var cartItems = await CartItemRepo.GetAsync(e=>productsIds.Contains(e.ProductId));            

            foreach (var p in products)
            {
                p.IsActive = false;
                p.IsDeleted = true;
            }

            await StoreRepo.CommitAsync();
            // ===================================================================================================================================
            return Ok(new {message = "Delete store successfuly!"});
        }
        #endregion
    }
}
