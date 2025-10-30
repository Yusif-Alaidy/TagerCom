using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace TagerCom.Area.Identity.Controller
{
    [Area("Identity")]
    [Route("api/identity/[controller]")]
    [ApiController]
    [Authorize]

    public class ProfilesController : ControllerBase
    {
        #region Fields
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository userRepository;
        private readonly ApplicationDbContext context;
        private readonly IRepository<UserAddress> addressRepository;
        #endregion

        #region Constructore
        public ProfilesController(UserManager<ApplicationUser> userManager, IUserRepository userRepository, ApplicationDbContext context, IRepository<UserAddress> addressRepository)
        {
            _userManager = userManager;
            this.userRepository = userRepository;
            this.context = context;
            this.addressRepository = addressRepository;
        }
        #endregion

        #region Profile Info
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            // Get current logged-in user ------------------------------------------------------------
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();
            // End Get current logged-in user --------------------------------------------------------

            // Get user with addresses using repository ------------------------------------------
            var userWithAddresses = await userRepository.GetUserWithAddressesAsync(currentUser.Id);
            if (userWithAddresses == null)
                return NotFound("User not found.");
            // End Get user with addresses -----------------------------------------------------------

            // Map to DTO (to avoid exposing Identity fields) ----------------------------------------
            var result = new
            {
                Id = userWithAddresses.Id,
                ProfileImg = userWithAddresses.ProfileImgUrl,
                FirstName = userWithAddresses.FirstName,
                LastName = userWithAddresses.LastName,
                PhonNumber = userWithAddresses.PhoneNumber,
                SecondPhonNumber = userWithAddresses.SecondPhoneNumber,
                Email = userWithAddresses.Email,
                UserName = userWithAddresses.UserName,
                Addresses = userWithAddresses.userAddresses.Select(a => new
                {
                    a.Id,
                    a.ApplicationUserId,
                    a.Label,
                    a.Country,
                    a.City,
                    a.Street,
                    a.ZipCode,
                    a.IsDefault
                })
            };
            // End Map to DTO ------------------------------------------------------------------------

            // 4️⃣ Return user profile with addresses -------------------------------------------------
            return Ok(result);
            // End Return user profile ---------------------------------------------------------------
        }
        #endregion

        #region Change Password
        [HttpPatch]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            // Get current logged-in user ------------====--------------------------------------------
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            // End Get current logged-in user --------------------------------------------------------

            // Change user password ------------------------------------------------------------------
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            // End Change user password --------------------------------------------------------------

            // Handle failed password update ----------------------------------------------------------
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            // End Handle failed password update -----------------------------------------------------

            // Return success message ----------------------------------------------------------------
            return Ok("Update Password Successfully");
            // End success message -------------------------------------------------------------------
        }
        #endregion

        #region Add Address
        [HttpPost("Address")]
        public async Task<IActionResult> AddAdress(AddressDTO request)
        {

            // Get current logged-in user ------------------------------------------------------------
            var user = await _userManager.GetUserAsync(User);
            // End Get current logged-in user --------------------------------------------------------

            // Map request data to UserAddress model -------------------------------------------------
            var address = new UserAddress
            {
                ApplicationUserId = user.Id,
                Label = request.Label,
                Country = request.Country,
                City = request.City,
                Street = request.Street,
                ZipCode = request.ZipCode,
                IsDefault = request.IsDefault
            };
            // End Map request data ------------------------------------------------------------------

            // Save address in database --------------------------------------------------------------
            await addressRepository.AddAsync(address);
            await addressRepository.CommitAsync();
            // End Save address ----------------------------------------------------------------------

            // Return success message ----------------------------------------------------------------
            return Ok(new { msg = "Address is Saved successfully" });
            // End success message -------------------------------------------------------------------

        }
        #endregion

        #region Update Profile
        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateInfo([FromForm] UpdateInformationRequest form)
        {
            // Retrieve the current user -----------------------------------------------
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();
            // --------------------------------------------------------------------------

            // Update fields only if data is provided -----------------------------------
            if (!string.IsNullOrEmpty(form.FirstName))
                user.FirstName = form.FirstName;

            if (!string.IsNullOrEmpty(form.LastName))
                user.LastName = form.LastName;

            if (!string.IsNullOrEmpty(form.PhoneNumber)) // We need send OTP To Confirem PhoneNumber
                user.PhoneNumber = form.PhoneNumber;

            if (!string.IsNullOrEmpty(form.SecondPhoneNumber))
                user.SecondPhoneNumber = form.SecondPhoneNumber;
            // -------------------------------------------------------------------------

            // Save uploaded image to wwwroot/img --------------------------------------
            if (form.ProfileImgUrl is not null)
            {

                var newFile = await SaveImageAsync(form.ProfileImgUrl);
                if (!string.IsNullOrEmpty(user.ProfileImgUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", user.ProfileImgUrl);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                user.ProfileImgUrl = newFile;
            }
            // --------------------------------------------------------------------------

            // Update user in database and return error message if update fails ---------
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            // --------------------------------------------------------------------------

            return Ok(new { msg = "Update Info Successfully" });
        }

        #endregion

        #region Helper
        
        // Save image in wwwroot/img folder -------------------------------------------------------
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            Directory.CreateDirectory(folderPath);

            // Security: Validate the file extension ---------------------------------------------
            var ext = Path.GetExtension(file.FileName);
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext.ToLowerInvariant()))
                throw new InvalidOperationException("Invalid image type.");
            // -----------------------------------------------------------------------------------

            // Generate unique name for the file -------------------------------------------------
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folderPath, fileName);
            // -----------------------------------------------------------------------------------

            // Maximum file size limit — for example, 20 MB --------------------------------------
            if (file.Length > 20 * 1024 * 1024)
                throw new InvalidOperationException("File too large.");
            // -----------------------------------------------------------------------------------

            // Save file to the target directory -------------------------------------------------
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            // -----------------------------------------------------------------------------------
            return fileName;
        }

        #endregion

    }
}
