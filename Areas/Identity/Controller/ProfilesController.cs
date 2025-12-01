using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Linq.Expressions;

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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<Store> _storeRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<Cart> _cartRepo;
        private readonly IRepository<CartItem> _cartItemRepo;
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<UserAddress> _userAddressRepo;
        private readonly ILogger<ProfilesController> _logger;
        private readonly IRepository<Wallet> walletRepo;

        #endregion

        #region Constructore
        public ProfilesController(
            UserManager<ApplicationUser>    userManager,
            IUserRepository                 userRepository,
            ApplicationDbContext            context,
            IRepository<UserAddress>        addressRepository,
            SignInManager<ApplicationUser>  signInManager,
            IRepository<Store>              storeRepo,
            IRepository<Product>            productRepo,
            IRepository<Order>              orderRepo,
            IRepository<Cart>               cartRepo,
            IRepository<CartItem>           cartItemRepo,
            IRepository<Review>             reviewRepo,
            IRepository<UserAddress>        userAddressRepo,
            ILogger<ProfilesController>     logger,
            IRepository<Wallet>             walletRepo
            )
        {
            _userManager            = userManager;
            this.userRepository     = userRepository;
            this.context            = context;
            this.addressRepository  = addressRepository;
            _signInManager          = signInManager;
            _storeRepo              = storeRepo;
            _productRepo            = productRepo;
            _orderRepo              = orderRepo;
            _cartRepo               = cartRepo;
            _cartItemRepo           = cartItemRepo;
            _reviewRepo             = reviewRepo;
            _userAddressRepo        = userAddressRepo;
            _logger                 = logger;
            this.walletRepo         = walletRepo;
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
                ApplicationUserId = user!.Id,
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

        #region Delete My Account
        [HttpDelete]
        public async Task<IActionResult> DeleteMyAccount()
        {
            try
            {
                // 1. Get current user ================================================
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Unauthorized(new { message = "User not found" });
                // ====================================================================

                // 2. Check if user has a store (is a vendor) =========================
                var store = await _storeRepo.GetOneAsync(
                    s => s.ApplicationUserId == user.Id,
                    includes: [e=>e.Orders,e => e.Products]);
                if (store != null)
                {
                    // Check if store has active orders
                    var hasActiveOrders = store.Orders.Any(o =>
                    o.OrderStatus != OrderStatus.Completed &&
                    o.OrderStatus != OrderStatus.Cancelled &&
                    o.OrderStatus != OrderStatus.Refunded);

                    if (hasActiveOrders)
                    {
                        return BadRequest(new
                        {
                            message = "Cannot delete account. You have active orders. Please complete or cancel them first."
                        });
                    }

                    // Get all cart items for store products
                    var productIds = store.Products.Select(p => p.Id).ToList();
                    var cartItems = await _cartItemRepo.GetAsync(
                    ci => productIds.Contains(ci.ProductId));

                    // Remove products from carts
                    await _cartItemRepo.DeleteRangeAsync(cartItems);
                    await _cartItemRepo.CommitAsync();

                    // Soft delete products
                    foreach (var product in store.Products)
                    {
                         product.IsActive = false;
                         product.IsDeleted = true;
                        _productRepo.Update(product);
                    }

                    // Soft delete store
                    store.IsActive = false;
                    store.IsDeleted = true;
                    _storeRepo.Update(store);

                    await _storeRepo.CommitAsync();
                    // Check if he have money in wallet 

                    var wallet = await walletRepo.GetOneAsync(e => e.UserId == user.Id);
                    if (wallet!.Balance > 0)
                    {
                        return BadRequest(new {message = "Cannot delete account. You have money in wallet" });
                    }
                    
                }
                // ====================================================================

                // 3. Handle user's orders (as a customer) ============================
                var customerOrders = await _orderRepo.GetAsync(
                o => o.CustomerId == user.Id);

                var hasActiveCustomerOrders = customerOrders.Any(o =>
                o.OrderStatus != OrderStatus.Completed &&
                o.OrderStatus != OrderStatus.Cancelled &&
                o.OrderStatus != OrderStatus.Refunded);

                if (hasActiveCustomerOrders)
                {
                    return BadRequest(new
                    {
                        message = "Cannot delete account. You have active orders as a customer."
                    });
                }

                foreach (var order in customerOrders)
                {
                    order.CustomerId = null;  // لو عملت nullable
                    _orderRepo.Update(order);
                }
                await _orderRepo.CommitAsync();
                // ====================================================================

                // 4. Handle user's cart ==============================================
                var cart = await _cartRepo.GetOneAsync(
                    c => c.UserId == user.Id,
                    includes: [ c => c.Items ]);

                if (cart != null)
                {
                    await _cartItemRepo.DeleteRangeAsync(cart.Items.ToList());
                    _cartRepo.Delete(cart);
                    await _cartRepo.CommitAsync();
                }
                // ====================================================================

                // 5. Handle reviews (delete them) ====================================
                var reviews = await _reviewRepo.GetAsync(
                    r => r.CustomerId == user.Id);

                if (reviews.Any())
                {
                    await _reviewRepo.DeleteRangeAsync(reviews);
                    await _reviewRepo.CommitAsync();
                }

                // Option: Keep reviews but anonymize
                // foreach (var review in reviews)
                // {
                //     review.CustomerId = "deleted-user";
                //     _reviewRepo.Update(review);
                // }
                // await _reviewRepo.CommitAsync();
                // ====================================================================

                // 6. Handle addresses ================================================
                var addresses = await _userAddressRepo.GetAsync(
                    a => a.ApplicationUserId == user.Id);

                if (addresses.Any())
                {
                    await _userAddressRepo.DeleteRangeAsync(addresses);
                    await _userAddressRepo.CommitAsync();
                }
                // ====================================================================

                // 7. Delete user account =============================================
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to delete account",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }
                // ====================================================================

                // 8. Sign out user ===================================================
                await _signInManager.SignOutAsync();

                return Ok(new
                {
                    message = "Account deleted successfully"
                });
                }
                // ====================================================================

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting user account");
                    return StatusCode(500, new
                    {
                        message = "An error occurred while deleting your account"
                    });
                }
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
