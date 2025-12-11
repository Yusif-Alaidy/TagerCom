using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TagerCom.Areas.Customer.DTOs.Request;

namespace TagerCom.Areas.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    [Area("Customer")]
    public class ReviewsController : ControllerBase
    {
        #region Fields
        private readonly IRepository<Review> _reviewRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Models.Store> _vendorRepository;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<Product> _productRepo;
        #endregion

        #region Ctor
        public Reviews_RatingsController(
            IRepository<Review> reviewRepository,
            UserManager<ApplicationUser> userManager,
            IRepository<Models.Store> vendorRepository,
            IRepository<Order> orderRepo,
            IRepository<Product> productRepo)
        {
            _reviewRepository = reviewRepository;
            _userManager = userManager;
            _vendorRepository = vendorRepository;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
        }
        #endregion

        #region Create Review
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] CreateReview dto)
        {
            // current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // must have bought product
            var ifbought = await _orderRepo.GetOneAsync(
                o => o.CustomerId == user.Id
                && (o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Completed)
                && o.OrderItems.Any(oi => oi.ProductId == dto.Productid)
               
            );

            if (ifbought == null)
                return BadRequest("You do not have Completed item in your orders");

            // prevent duplicate review
            var reviewed = await _reviewRepository.GetOneAsync(
                r => r.CustomerId == user.Id && r.ProductId == dto.Productid,
                includes: null,
                tracked: false
            );

            if (reviewed != null)
                return BadRequest("You Already Reviewed This Product");

            // create review
            var review = new Review
            {
                CustomerId = user.Id,
                ProductId = dto.Productid,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.CommitAsync();

            return Ok(new { msg = "Review Submitted Successfully" });
        }
        #endregion

        #region Ratings Summary + Pagnation
        [HttpGet("show-reviews")]
        public async Task<IActionResult> GetProductReviews([FromQuery] ProductReviewsDTO dto)
        {

            // validate product
            var productExists = await _productRepo.GetOneAsync(
                p => p.Id == dto.ProductId && !p.IsDeleted && p.IsActive
            );

            if (productExists == null)
                return NotFound("Product not found");

            var reviewsQuery = _reviewRepository.Query()
                .AsNoTracking()
                .Where(r => r.ProductId == dto.ProductId);

            //
            var totalCount = await reviewsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize);

            var reviews = await reviewsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(r => new
                {
                    r.Id,
                    r.CustomerId,
                    r.Rating,
                    Comment = r.Comment.Trim(),
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                productId = dto.ProductId,
                page = dto.Page,
                pageSize = dto.PageSize,
                totalCount,
                totalPages,
                reviews
            });
        }

        #endregion

        #region Update Review
        [HttpPut]
        public async Task<IActionResult> UpdateMyReviewForProduct([FromQuery] Guid productId, [FromBody] UpdateReviewDTO dto)
        {
            // current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();


            // validate product
            var product = await _productRepo.GetOneAsync(
                p => p.Id == productId && !p.IsDeleted && p.IsActive
            );

            if (product == null)
                return NotFound("Product not found");

            // must have bought product
            var hasBought = await _orderRepo.GetOneAsync(
                o => o.CustomerId == user.Id
                 && (o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Completed)
                 && o.OrderItems.Any(oi => oi.ProductId == productId)

            );

            if (hasBought == null)
                return BadRequest("You can only update reviews for products you purchased");

            // load review
            var review = await _reviewRepository.GetOneAsync(
                r => r.ProductId == productId && r.CustomerId == user.Id
            );

            if (review == null)
                return NotFound("You don't have a review for this product");

            // update
            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim();

            await _reviewRepository.CommitAsync();
            return Ok(new { message = "Review updated" });
        }
        #endregion

        #region Delete Review
        [HttpDelete]
        public async Task<IActionResult> DeleteMyReviewForProduct([FromQuery] Guid productId)
        {
            // current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // validate product
            var product = await _productRepo.GetOneAsync(
                p => p.Id == productId && !p.IsDeleted && p.IsActive
            );

            if (product == null)
                return NotFound("Product not found.");

            // must have bought product
            var hasBought = await _orderRepo.GetOneAsync(
                o => o.CustomerId == user.Id
                && (o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Completed)
                && o.OrderItems.Any(oi => oi.ProductId == productId)
               
            );

            if (hasBought == null)
                return BadRequest("You can only delete reviews for products you purchased");

            // load review
            var review = await _reviewRepository.GetOneAsync(
                r => r.ProductId == productId && r.CustomerId == user.Id,
                tracked: true
            );

            if (review == null)
                return NotFound("You don't have a review for this product");

            // delete
            _reviewRepository.Delete(review);
            await _reviewRepository.CommitAsync();

            return Ok(new { message = "Review deleted" });
        }
        #endregion
    }
}
