using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


        public CustomersController(IRepository<Favorite> favoriteRepo,
            IRepository<Product> productRepo, UserManager<ApplicationUser> userManager)
        {
            _favoriteRepository=favoriteRepo;
            _productRepo=productRepo;
            _userManager=userManager;
        }

        [Authorize(Roles ="Customer")]
        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody]AddFavoriteRequestDTO addFavorite)


        {
            var user = await _userManager.GetUserAsync(User);

            var productexist = _favoriteRepository.Query().
                Any(f => f.ApplicationUserId == user.Id && f.ProductId == addFavorite.ProductId);

            if (productexist)
                return Conflict(new { message = "Product is already in Your favorites." });

            var addingproduct = _productRepo.GetOneAsync(p => p.Id == addFavorite.ProductId);
            if (addingproduct == null)
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


        [HttpDelete("{productId:int}")]
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



    }
}
