using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TagerCom.Areas.Customer.DTOs.Request;
using TagerCom.Areas.Customer.DTOs.Response;
using TagerCom.Models;

namespace TagerCom.Areas.Customer.Controllers
{
    [Route("api/customer/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class ValuesController : ControllerBase
    {
        #region Fields
        public IRepository<Product> ProductRepo { get; }
        public IRepository<Review> ReviewRepo { get; }
        #endregion

        #region Constructore
        public ValuesController(IRepository<Product> ProductRepo, IRepository<Review> ReviewRepo)
        {
            this.ProductRepo = ProductRepo;
            this.ReviewRepo = ReviewRepo;
        }

        #endregion

        #region Get All Product
        [HttpGet]
        public async Task<IActionResult> GetAllProduct([FromQuery] ProductFilterRequest request)
        {

            // Get All Prodcut -----------------------------------------
            var Products = await ProductRepo.GetAsync(e => e.Stock >= 1 && e.IsActive == true);
            // ---------------------------------------------------------

            // Search with name and description ------------------------
            if (request.Search is not null)
                Products = await ProductRepo.GetAsync(e => e.Name.Contains(request.Search) || e.Description.Contains(request.Search) && e.Stock >= 1 && e.IsActive == true);
            // ---------------------------------------------------------

            // Filter --------------------------------------------------
            if (!string.IsNullOrEmpty(request.Category))
                Products = await ProductRepo.GetAsync(e => e.Category.Name.Contains(request.Category));
            if (request.MinPrice.HasValue)
                Products = await ProductRepo.GetAsync(e => e.Price >= request.MinPrice);
            if (request.MaxPrice.HasValue)
                Products = await ProductRepo.GetAsync(e => e.Price <= request.MaxPrice);
            // ---------------------------------------------------------

            // Sorting -------------------------------------------------
            switch (request.SortBy?.ToLower())
            {
                case "price":
                    Products = request.descending == true ? Products.OrderByDescending(e => e.Price).ToList() : Products.OrderBy(e => e.Price).ToList();
                    break;
                case "date":
                    Products = request.descending == true ? Products.OrderByDescending(e => e.CreatedAt).ToList() : Products.OrderBy(e => e.CreatedAt).ToList();
                    break;
                case "rate":
                    Products = request.descending == true ? Products.OrderByDescending(e => e.Rate).ToList() : Products.OrderBy(e => e.Rate).ToList();
                    break;
                default:
                    // Default sorting: Newest first
                    Products = Products.OrderBy(p => p.CreatedAt).ToList();
                    break;
            }
            // ---------------------------------------------------------

            // Pagination ----------------------------------------------
            var totalNumberOfPages = Math.Ceiling(Products.Count() / 2.0);
            var currentPage = request.currentPage;
            Products = Products.Skip(( request.currentPage - 1 ) * 2).Take(2).ToList();
            // ---------------------------------------------------------

            // Mapping -------------------------------------------------
            var ProductsDTO = Products.Select(e => new ProductsRespons
            {
                Id          = e.Id,
                Name        = e.Name,
                VendorId    = e.VendorId,
                CategoryId  = e.CategoryId,
                Description = e.Description,
                Price       = e.Price,
                Stock       = e.Stock,
                ImageUrl    = e.ImageUrl,
                IsActive    = e.IsActive,
                CreatedAt   = e.CreatedAt
            });
            // ---------------------------------------------------------


            return Ok(new {
                Products = ProductsDTO,
                TotalNumberOfPages = totalNumberOfPages,
                CurrentPage = currentPage,
                Search = request.Search,
                MaxPrice = request.MaxPrice,
                MinPrice = request.MinPrice,
                Category = request.Category,
                SortBy = request.SortBy,
                Descending = request.descending
            });
        }
        #endregion

        #region Get By Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            // Get the Product ---------------------------
            var Product = await ProductRepo.GetOneAsync(e => e.Id == id, includes:[e=> e.Reviews]);
            if(Product == null)
                return NotFound();
            // -------------------------------------------

            // Get all reviews for this product ----------
            var Reviews = await ReviewRepo.GetAsync(e => e.Product.Id == Product.Id);
            // -------------------------------------------

            // Mapping -----------------------------------
            var ProductDto = new ProductsRespons
            {
                Id          = Product.Id,
                Name        = Product.Name,
                VendorId    = Product.VendorId,
                CategoryId  = Product.CategoryId,
                Description = Product.Description,
                Price       = Product.Price,
                Stock       = Product.Stock,
                ImageUrl    = Product.ImageUrl,
                IsActive    = Product.IsActive,
                CreatedAt   = Product.CreatedAt
            };

            var ReviewDTO = Reviews.Select(e => new Review
            {
                Id = e.Id,
                Rating = e.Rating,
                Comment = e.Comment,
                CreatedAt = e.CreatedAt,
            });
            // -------------------------------------------

            return Ok(new
            {
                Products = ProductDto,
                Reviews = ReviewDTO,
            });
        }
        #endregion

        #region Get Similar Product
        [HttpGet("similar/{id:int}")]
        public async Task<IActionResult> GetSimilar(int id) 
        {
            // Get Product -------------------------------------------
            var Product = await ProductRepo.GetOneAsync(e=>e.Id == id);
            if (Product == null)
                return NotFound(new {msg = "This product is not exist"});
            // ------------------------------------------------------

            // Get Similar ------------------------------------------
            var SimilarProduct = await ProductRepo.GetAsync(e=>e.CategoryId == Product.CategoryId && e.Stock >= 1 && e.IsActive);
            if (Product == null)
                return NotFound(new {msg = "Have no similar product"});
            // ------------------------------------------------------


            // Mapping ----------------------------------------------
            var ProductsDTO = SimilarProduct.Select(e => new ProductsRespons
            {
                Id          = e.Id,
                Name        = e.Name,
                VendorId    = e.VendorId,
                CategoryId  = e.CategoryId,
                Description = e.Description,
                Price       = e.Price,
                Stock       = e.Stock,
                ImageUrl    = e.ImageUrl,
                IsActive    = e.IsActive,
                CreatedAt   = e.CreatedAt
            });
            // -------------------------------------------------------

            return Ok(new {Similar_Product = ProductsDTO });
        }

        #endregion

        #region Get Best Sellers

        [HttpGet("bestsellers")]
        public async Task<IActionResult> GetBestsellers() 
        {
            // Get All Products -------------------------------------------------------
            var AllProducts = await ProductRepo.GetAsync(includes:[e=>e.OrderItems]);
            if (AllProducts == null)
                return NotFound(new { msg = "There are no products"});
            // ------------------------------------------------------------------------

            // Get Best Sellers -------------------------------------------------------
            var Products = AllProducts
                .Select(p=> new
                {
                    p.Id,
                    p.Name,
                    p.VendorId,
                    p.CategoryId,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.ImageUrl,
                    p.IsActive,
                    p.CreatedAt,
                    TotalSold = p.OrderItems.Sum(oi => (int?)oi.Quantity) })
                .OrderByDescending(e=> e.TotalSold)
                .Take(10)
                .ToList();
            // -------------------------------------------------------------------------

            // Mapping -----------------------------------------------------------------
            var ProductsDTO = Products.Select(e => new ProductsRespons
            {
                Id          = e.Id,
                Name        = e.Name,
                VendorId    = e.VendorId,
                CategoryId  = e.CategoryId,
                Description = e.Description,
                Price       = e.Price,
                Stock       = e.Stock,
                ImageUrl    = e.ImageUrl,
                IsActive    = e.IsActive,
                CreatedAt   = e.CreatedAt,
                TotalSold   = e.TotalSold,
            });
            // -------------------------------------------------------


            return Ok(new { Products = ProductsDTO });
        }
        #endregion

    }
}