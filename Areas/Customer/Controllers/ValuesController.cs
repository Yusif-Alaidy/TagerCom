using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
        public IRepository<Models.Store> StoreRepo { get; }
        #endregion

        #region Constructore
        public ValuesController(IRepository<Product> ProductRepo, IRepository<Review> ReviewRepo, IRepository<TagerCom.Models.Store> StoreRepo)
        {
            this.ProductRepo = ProductRepo;
            this.ReviewRepo = ReviewRepo;
            this.StoreRepo = StoreRepo;
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
                    Products = request.descending == true ? Products.OrderByDescending(e => e.Reviews).ToList() : Products.OrderBy(e => e.Reviews).ToList();
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
                VendorId    = e.StoreId,
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
        [HttpGet("id")]
        public async Task<IActionResult> GetOne(Guid id)
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
                VendorId    = Product.StoreId,
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
        [HttpGet("similar/{id}")]
        public async Task<IActionResult> GetSimilar(Guid id) 
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
                VendorId    = e.StoreId,
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
                    p.StoreId,
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
                VendorId    = e.StoreId,
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

        #region Visit Store
        [HttpGet("Visit-Store/{storeId}")]
        public async Task<IActionResult> VisitStore(
            [FromRoute] Guid storeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? sortBy = "newest"
            )
        {
            // Get Store =======================
            var store = await StoreRepo.GetOneAsync(e=>e.Id == storeId);
            if (store==null)
            {
                return NotFound(new {message = "This stor is not exist"});
            }
            if (!store.IsActive || store.Status != StoreStatus.Approved || store.IsDeleted)
            {
                return BadRequest(new { message = "This Store Is not Exit Anymore" });
            }
            // =================================

            // Get Product In Store ============
            var query = ProductRepo.Query()
            .Where(p => p.StoreId == storeId && p.IsActive && p.IsDeleted == false);
            // =================================

            // Apply Filters
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category.Name == category);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Apply Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "oldest" => query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // newest
            };

            // Get Paginated Products
            var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductsRespons
            {
                Id = p.Id,
                VendorId = p.StoreId,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock,
                Category = p.Category.Name
            })
            .ToListAsync();

            // Mapping =========================
            // check products are active and add pagination
            var response = new VisitStoreResponse
            {
                StoreName = store.StoreName,
                Rating = store.Rating,
                products = products
            };
            // =================================

            return Ok(response);
        }

        #endregion
    }
}
