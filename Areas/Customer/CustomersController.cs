using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TagerCom.Models;
namespace TagerCom.Areas.Customer
{
    [Route("api/Customer/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {

        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<Product> _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Wishlist> _wishlistRepository;
        private readonly IRepository<UserAddress> _userAddress;


        public CustomersController(IRepository<Favorite> favoriteRepo,
            IRepository<Product> productRepo, UserManager<ApplicationUser> userManager, IRepository<Wishlist> wishlistRepository)
        {
            _favoriteRepository=favoriteRepo;
            _productRepo=productRepo;
            _userManager=userManager;
            _wishlistRepository=wishlistRepository;

        }


        [Authorize(Roles = "Customer")]
        [HttpGet("GetMyFavorites")]
        public async Task <IActionResult>GetMyFavorites()
        {
            var user = await _userManager.GetUserAsync(User);

            var favorites = _favoriteRepository.Query().Where(f => f.ApplicationUserId == user.Id)
                .Select(f => new FavoriteResponseDTO
                {
                    Id = f.Id,
                    ProductId = f.ProductId,
                    ProductName = f.Product.Name,
                    ImageUrl = f.Product.ImageUrl,
                    Price = f.Product.Price,
                    CreatedAt = f.CreatedAt


                }).ToList();
            return Ok(favorites);


        }

        [Authorize(Roles = "Customer")]
        [HttpPost("AddToFavorites")]
        public async Task<IActionResult> AddToFavorites([FromBody] AddFavoriteRequestDTO addFavorite)
        {
            //var userId = "c2744f14-ac46-42b5-b205-1006910e12fb"; // Hardcoded Customer test by id
            //var productExist = _favoriteRepository.Query()
            //.Any(f => f.ApplicationUserId == userId && f.ProductId == addFavorite.ProductId);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "You must be logged in as Customer." });

            var productExist = _favoriteRepository.Query()
                .Any(f => f.ApplicationUserId == user.Id && f.ProductId == addFavorite.ProductId);

            if (productExist)
                return Conflict(new { message = "Product is already in Your favorites." });

            var addingProduct = await _productRepo.Query()
                .FirstOrDefaultAsync(p => p.Id == addFavorite.ProductId);

            if (addingProduct == null)
                return NotFound(new { message = "Product not found." });

            var favorite = new Favorite
            {
                ApplicationUserId = user.Id,
                ProductId = addFavorite.ProductId,
                CreatedAt = DateTime.UtcNow
            };

            await _favoriteRepository.AddAsync(favorite);
            await _favoriteRepository.CommitAsync();

            return Ok(new { message = "Product added to Your favorites successfully." });
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("RemoveFromFavorites/{productId:int}")]
        public async Task<IActionResult> RemoveFromFavorites(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            var favorite = _favoriteRepository.Query()
                .FirstOrDefault(f => f.ApplicationUserId == user.Id && f.ProductId == productId);

            if (favorite == null)
                return NotFound(new { message = "Product not found in favorites." });

            _favoriteRepository.Delete(favorite);
            await _favoriteRepository.CommitAsync();

            return Ok(new { message = "Product removed from favorites successfully." });
        }


        #region Add to Wishlist
        [Authorize(Roles = "Customer")]
        [HttpPost("AddMyWishlist")]
        public async Task<IActionResult> AddToWishlist(WishlistRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { msg = "User not found" });

            //var address = await _userAddress.GetAsync(e => e.ApplicationUserId == user.Id);
            //if (string.IsNullOrEmpty(user.PhoneNumber) || string.IsNullOrEmpty(user.FirstName) || address == null)
            //{
            //    return BadRequest(new { msg = "Your profile is not complete" });
            //}

            //  Checking Product
            var product = await _productRepo.GetAsync(p => p.Id == request.ProductId);
            if (product == null)
                return NotFound(new { msg = "Product not found" });

                    if (user == null)
                return BadRequest(new { msg = "User not found" });

            //  Checking request 
            if (request == null || request.ProductId <= 0)
                return BadRequest(new { msg = "Invalid product" });
              // Checking of the wishlist is already exists
            var exists = await _wishlistRepository.Query()
                .FirstOrDefaultAsync(w => w.ApplicationUserId == user.Id && w.ProductId == request.ProductId);

            if (exists != null)
                return BadRequest(new { msg = "Product already in Wishlist" });

            // making a new wishlist
            var wishlistItem = new Wishlist
            {
                ApplicationUserId = user.Id,
                ProductId = request.ProductId,
                CreatedAt = DateTime.UtcNow
            };

            // Saving wishlist
            await _wishlistRepository.AddAsync(wishlistItem);
            await _wishlistRepository.CommitAsync(); // تأكد إن CommitAsync فعليًا يعمل SaveChangesAsync

            return Ok(new { msg = "Product added to Wishlist" });
        }
        #endregion


        #region Get Wishlist
        [Authorize(Roles = "Customer")]
        [HttpGet("GetMyWishlist")]
        public async Task<ActionResult<List<WishlistItemDTO>>> GetWishlist()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { msg = "User not found" });

            // include expression can handle null without null references error
            var wishlist = await _wishlistRepository.GetAsync(
                expression: w => w.ApplicationUserId == user.Id,
                includes: new Expression<Func<Wishlist, object>>[] { w => w.Product }
            );

            if (wishlist == null || !wishlist.Any())
                return Ok(new { msg = "Your wishlist is empty" });

            // transfer the wishlist to DTO
            var wishlistDto = wishlist.Select(w => new WishlistItemDTO
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product?.Name,
                ProductImage = w.Product?.ImageUrl,
                ProductPrice = w.Product?.Price ?? 0,
                ProductDescription = w.Product?.Description,
                CreatedAt = w.CreatedAt
            }).ToList();

            return Ok(wishlistDto);
        }
        #endregion


    }
}
