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
    public class StoreController : ControllerBase
    {
        #region Fields
        public UserManager<ApplicationUser> UserManager { get; }
        public IRepository<Models.Store> StoreRepo { get; }
        #endregion

        #region Constructore
        public StoreController(UserManager<ApplicationUser> UserManager, IRepository<Models.Store> StoreRepo)
        {
            this.UserManager = UserManager;
            this.StoreRepo = StoreRepo;
        }

        #endregion

        #region view store
        [HttpGet]
        public async Task<IActionResult> GetStore()
        {
            // Get user ================================================
            var user = await UserManager.GetUserAsync(User);
            // =========================================================

            // Get Store ===============================================
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
                RevenueShare    = store.RevenueShare,
                CreatedAt       = store.CreatedAt,
            };
            // =========================================================
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
    }
}
