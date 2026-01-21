using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TagerCom.Areas.Admin.DTOs.Requests;

namespace TagerCom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {

        #region Fields
        public UserManager<ApplicationUser> UserManager { get; }
        public IRepository<Models.Store> StoreRepo { get; }
        public IRepository<Product> ProductRepo { get; }
        public IRepository<CartItem> CartItemRepo { get; }
        #endregion

        #region Constructore
        public UsersController(UserManager<ApplicationUser> userManager, IRepository<Models.Store> StoreRepo, IRepository<Product> ProductRepo, IRepository<CartItem> CartItemRepo)
        {
            this.UserManager = userManager;
            this.StoreRepo = StoreRepo;
            this.ProductRepo = ProductRepo;
            this.CartItemRepo = CartItemRepo;
        }
        #endregion

        #region Get all users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetUsersRequest request)
        {
            // Get Users ============================================================
            // ----------
            var users = UserManager.Users;
            if (users == null || users.ToList().Count == 0)
                return NotFound();
            // ======================================================================

            // Filter ===============================================================
            // -------
            if (!string.IsNullOrEmpty(request.email))
                users = UserManager.Users.Where(e => e.Email == request.email);

            if (!string.IsNullOrEmpty(request.userName))
                users = UserManager.Users.Where(e => e.UserName == request.userName);
            // ======================================================================

            // Pagination ===========================================================
            // -----------
            users = users.Take(request.pageSize).Skip(request.pageSize * ( request.page - 1 ));
            // ======================================================================

            return Ok(users);
        }
        #endregion

        #region Get user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllUsers(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("id is required");

            // Get Users ============================================================
            // ----------
            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            // ======================================================================
            return Ok(user);
        }
        #endregion

        #region Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {

            // id Check ===========================================================================================================================
            // ---------
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("id is required.");

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            // ====================================================================================================================================

            // this admin can't delete his account ================================================================================================
            // ------------------------------------
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId) && currentUserId == user.Id)
                return BadRequest("You cannot delete your own account.");
            // ====================================================================================================================================

            // delete account =====================================================================================================================
            // ---------------

            var result = await UserManager.DeleteAsync(user);
            // ====================================================================================================================================

            // result =============================================================================================================================
            // -------
            if (!result.Succeeded)
            {
                // رجّع أسباب الفشل
                return BadRequest(new
                {
                    message = "Failed to delete user.",
                    errors = result.Errors.Select(e => new { e.Code, e.Description })
                });
            }
            // ====================================================================================================================================
            // Get store ==========================================================================================================================
            // ====================================================================================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == id);
            if (store is not null)
            {
                // Chech if can delete ============================================================================================================
                // ================================================================================================================================
                var hasActiveOrder = store.Orders.Any(e=>
                    e.OrderStatus != OrderStatus.Refunded  &&
                    e.OrderStatus != OrderStatus.Cancelled &&
                    e.OrderStatus != OrderStatus.Completed );
                if (hasActiveOrder)
                {
                    return BadRequest(new
                    {
                        message = "Cannot delete account. This store have active orders."
                    });
                }
                // ================================================================================================================================


                // Delete Proccess ================================================================================================================
                // ================================================================================================================================
                store.IsDeleted = true;
                store.IsActive = false;

                var products = await ProductRepo.GetAsync(equals=>equals.StoreId == store.Id && !equals.IsDeleted);
                var productsIds = products.Select(e => e.Id);

                var cartItems = await CartItemRepo.GetAsync(e=>productsIds.Contains(e.ProductId));

                foreach (var p in products)
                {
                    p.IsActive = false;
                    p.IsDeleted = true;
                }

                await StoreRepo.CommitAsync();
            }
            // ===================================================================================================================================


            // REST الأفضل 204
            return NoContent();
        }
        #endregion

        #region Change user role to customer service
        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeRole(string id)
        {
            // Get user ===================================================
            // ---------
            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return BadRequest(new {message = "this user is not exist"});
            // ============================================================

            // Change Role ================================================
            // ------------
            if (await UserManager.IsInRoleAsync(user, "Vendor"))
            { 
                return BadRequest(new {message = "this user aleardy vendor"});
            }

            await UserManager.AddToRoleAsync(user, "CustomerService");
            await ProductRepo.CommitAsync();
            // ============================================================

            return Ok(new {message = "this user is customer serivce now"});
        }
        #endregion
    }
}
