using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TagerCom.Areas.Store.DTOs.Response;

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
            var store = (await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id));

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
    }
}
