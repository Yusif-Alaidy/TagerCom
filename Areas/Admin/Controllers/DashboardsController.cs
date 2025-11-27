using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TagerCom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardsController : ControllerBase
    {
        #region Fields
        public IRepository<Models.Store> Vendor { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        #endregion

        #region Constructore
        public DashboardsController(IRepository<Models.Store> vendor, UserManager<ApplicationUser> userManager)
        {
            this.Vendor = vendor;
            this.UserManager = userManager;
        }

        #endregion

        #region Get All pending vendors

        [HttpGet("Vendor")]
        public async Task<IActionResult> GetPendingVendor([FromQuery] VendorPendingFilter? filter, [FromQuery]int page = 1)
        {

            // 1. Retrieve all vendors with pending status ----------------------------------------------------------------------------
            var PendingVendors = await Vendor.GetAsync(e=>e.Status == 0,includes:[e=>e.ApplicationUser]);
            if (PendingVendors == null || PendingVendors.Count < 1)
                return Ok(new { msg = "No Pending vandor is exist" });
            // ------------------------------------------------------------------------------------------------------------------------


            // 2. Apply filtering by Username, Email, PhoneNumber, or StoreName -------------------------------------------------------
            if (filter!.username != null)
            {
                PendingVendors = await Vendor.GetAsync(e => e.ApplicationUser.UserName!.Contains(filter.username));
            }
            if (filter.email != null)
            {
                PendingVendors = await Vendor.GetAsync(e => e.ApplicationUser.Email!.Contains(filter.email));

            }
            if (filter.phoneNumber != null)
            {
                if (( PendingVendors = await Vendor.GetAsync(e => e.ApplicationUser.PhoneNumber!.Contains(filter.phoneNumber)) ).Count > 0)
                    PendingVendors = await Vendor.GetAsync(e => e.ApplicationUser.PhoneNumber!.Contains(filter.phoneNumber));
                else
                    PendingVendors = await Vendor.GetAsync(e => e.ApplicationUser.SecondPhoneNumber!.Contains(filter.phoneNumber));
            }
            if (filter.StoreName != null)
            {
                PendingVendors = await Vendor.GetAsync(e => e.StoreName.Contains(filter.StoreName));
            }
            // ----------------------------------------------------------------------------------------------------------------------------


            // 3. Pagination --------------------------------------------------------------------------------------------------------------
            // Order first, then paginate

            var totalNumberOfPages = Math.Ceiling(PendingVendors.Count() / 10.0);
            var currentPage = page;

            PendingVendors = PendingVendors
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToList();
            // ----------------------------------------------------------------------------------------------------------------------------


            // 5. Map results to DTOs to prevent circular reference issues ----------------------------------------------------------------
            var PendingVendorsDTO = PendingVendors.Select(e => new PendingVendorResponse
            {
                ApplicationUserId   = e.ApplicationUserId,
                vendoreId           = e.Id,
                Username            = e.ApplicationUser.UserName!,
                Email               = e.ApplicationUser.Email!,
                StoreName           = e.StoreName,
                phoneNumber         = e.ApplicationUser.PhoneNumber!,
                SecondPhoneNumber   = e.ApplicationUser.SecondPhoneNumber,
                CreatedAt           = e.CreatedAt,
                Status              = e.Status.ToString(),
            });
            // ----------------------------------------------------------------------------------------------------------------------------

            return Ok(new {
                data                = PendingVendorsDTO,
                currentPage         = currentPage,
                totalNumberOfPages  = totalNumberOfPages,
            });
        }

        #endregion

        #region Approved/Rejected vendors

        [HttpPatch("status")]
        public async Task<IActionResult> UpdateVendorStatus([FromBody]UpdateVendorStatusRequest request)
        {

            // Get spcific pending vendor --------------------------
            var pendingVendor = await Vendor.GetOneAsync(e=>e.Id == request.VendorId,includes:[e=>e.ApplicationUser]);
            if (pendingVendor == null)
                return NotFound(new {msg = "this store is not found"});
            //------------------------------------------------------


            // Approved --------------------------------------------
            if (request.ApprovedOrRejected.ToString() == "Approved")
            {
                pendingVendor.Status = StoreStatus.Approved;
                await UserManager.AddToRoleAsync(pendingVendor.ApplicationUser, "Vendor");
                return Ok(new
                {
                    msg = "Successfuly Welcome in our familey now you are a vendor"
                });
                
            }
            // -----------------------------------------------------


            // Rejected --------------------------------------------
            if (request.ApprovedOrRejected.ToString() == "Rejected")
            {
                pendingVendor.Status = StoreStatus.Rejected;
                return Ok(new
                {
                    msg = "Sorry your store is rejected mybe you can try again"
                });
            }
            // -----------------------------------------------------
            return BadRequest();
        }

        #endregion
    }
}
