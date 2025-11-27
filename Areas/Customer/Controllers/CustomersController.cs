using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Linq.Expressions;
using TagerCom.Models;
namespace TagerCom.Areas.Customer.Controllers
{
    [Route("api/Customer/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {

        private readonly IRepository<Favorite> _favoriteRepository;
        private readonly IRepository<Product> _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Wishlist> _wishlistRepository;


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

            var favorites = _favoriteRepository.Query().Where(f => f.ApplicationUserId == user!.Id)

                //new FavoriteResponseDTO بسجل القيم اللي فيه بس كده 
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




            // Using GetOneAsync
            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //    return NotFound();
            //var favorite = (await _favoriteRepository.GetOneAsync(z => z.ApplicationUserId == user.Id)).
            //    (z => new  FavoriteResponseDTO

            //{
            //    Id = favorite.Id,
            //    ProductId = favorite.ProductId,
            //    ProductName = favorite.Product.Name,
            //    ImageUrl = favorite.Product.ImageUrl,
            //    Price = favorite.Product.Price,
            //    CreatedAt = favorite.CreatedAt


            //});


            //var aa = (await _favoriteRepository.GetAsync(z => z.ApplicationUserId == user.Id)).Select(z=> new
            //{
            //    z.ApplicationUserId,

            //})
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
        [HttpDelete("RemoveFromFavorites/{productId}")]
        public async Task<IActionResult> RemoveFromFavorites(Guid productId)
        {
            var user = await _userManager.GetUserAsync(User);

            var favorite = _favoriteRepository.Query()
                .FirstOrDefault(f => f.ApplicationUserId == user!.Id && f.ProductId == productId);

            if (favorite == null)
                return NotFound(new { message = "Product not found in favorites." });

            _favoriteRepository.Delete(favorite);
            await _favoriteRepository.CommitAsync();

            return Ok(new { message = "Product removed from favorites successfully." });
        }


       


    }
}
