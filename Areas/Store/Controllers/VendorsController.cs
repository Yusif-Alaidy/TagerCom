using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TagerCom.Areas.Store.Controllers
{
    [Area("Store")]
    [Route("api/store/[controller]")]
    [ApiController]
    [Authorize]
    public class VendorsController : ControllerBase
    {
        #region Fields
        public UserManager<ApplicationUser> userManager { get; }
        public IRepository<UserAddress> userAddress { get; }
        public IRepository<Vendor> VendoreRepsitory { get; }
        #endregion

        #region Constructore
        public VendorsController(UserManager<ApplicationUser> userManager, IRepository<UserAddress> userAddress, IRepository<Vendor> vendoreRepsitory)
        {
            this.userManager = userManager;
            this.userAddress = userAddress;
            this.VendoreRepsitory = vendoreRepsitory;
        }
        #endregion

        #region Vendor Register

        [HttpPost]
        public async Task<IActionResult> VendorRegsiter(VendorRegisterRequest request)
        {
            // Get This User ----------------------------------------------
            var user = await userManager.GetUserAsync(User);
            // ------------------------------------------------------------


            // Check if this user has complete profile --------------------
            var address = userAddress.GetAsync(e => e.ApplicationUserId == user.Id);
            if (user.PhoneNumber == null && user.FirstName == null && user.FirstName == null && user.PhoneNumber == "" && user.FirstName == "" && user.FirstName == "" && address == null)
            {
                return BadRequest(new {msg = "Your Profile is not complete"});
            }
            // ------------------------------------------------------------


            // Create new vendor for this user and change status  ---------
            var vendor = new Vendor()
            {
                ApplicationUserId = user.Id,
                CompanyName = request.CompanyName,
                CreatedAt = DateTime.UtcNow,
                Status = VendorStatus.Pending,
            };
            await VendoreRepsitory.AddAsync(vendor);
            await VendoreRepsitory.CommitAsync();

            // ------------------------------------------------------------

            return Ok(new {msg = "Your Request is Send to Admin Succuss"});
        }

        #endregion
    }
}
