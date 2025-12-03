using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TagerCom.Areas.Customer.DTOs.Request;

namespace TagerCom.Areas.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("Customer")]

    public class Reviews_RatingsController : ControllerBase
    {

        private readonly IRepository<Review> _reviewRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Models.Store> _vendorRepository;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<Product> _productRepo;

        public Reviews_RatingsController(IRepository<Review> _ReviewRepository, UserManager<ApplicationUser> UserManager,
            IRepository<Models.Store> _VendorRepository, IRepository<Order> _orderRepo, IRepository<Product> _productRepo) 
        
        {
            this._reviewRepository= _ReviewRepository;
            this._vendorRepository= _VendorRepository;
            this._userManager = UserManager;
            this._orderRepo= _orderRepo;
            this._productRepo=_productRepo;
        }


        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody]CreateReview dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();

            var ifbought = await _orderRepo.GetOneAsync(
                o => o.CustomerId == user.Id
                     && (o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Completed)
                     && o.OrderItems.Any(oi => oi.ProductId == dto.Productid),
                   includes: null,
                   tracked: false
                   );

            if (ifbought == null)
                return BadRequest("You do not have Completed item in your orders");


            var Reviewed = await _reviewRepository.GetOneAsync(r => r.CustomerId == user.Id && r.ProductId == dto.Productid,
                   includes: null,
                   tracked: false

            );

            if (Reviewed!=null)
                return BadRequest("You Already Reviewed This Product");



            var Review = new Review
            {
                Id = Guid.NewGuid(),
                CustomerId = user.Id,
                ProductId = dto.Productid,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,


            };

            await _reviewRepository.AddAsync(Review);
                await _reviewRepository.CommitAsync();

            return Ok(new {msg="Review Submitted Successfully" });

        }



       



    }
}
