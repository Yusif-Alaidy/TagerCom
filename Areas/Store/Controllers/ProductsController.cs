using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TagerCom.Areas.Store.DTOs.Request;
using TagerCom.Areas.Store.DTOs.Response;

namespace TagerCom.Areas.Store.Controllers
{
    [Area("Store")]
    [Route("api/store/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendor")]
    public class ProductsController : ControllerBase
    {
        #region Fields
        public UserManager<ApplicationUser> userManager { get; }
        public IRepository<Models.Store> StoreRepo { get; }
        public IRepository<Product> ProductRepo { get; }
        public IRepository<OrderItem> OrderItemRepo { get; }
        public IRepository<Review> ReviewRepo { get; }
        #endregion

        #region Constructore
        public ProductsController(UserManager<ApplicationUser> userManager, IRepository<Models.Store> storeRepo, IRepository<Product> productRepo, IRepository<OrderItem> orderItemRepo, IRepository<Review> reviewRepo)
        {
            this.userManager = userManager;
            this.StoreRepo = storeRepo;
            this.ProductRepo = productRepo;
            this.OrderItemRepo = orderItemRepo;
            this.ReviewRepo = reviewRepo;
        }

        #endregion

        #region Create Product
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductsRequest request)
        {
            // Get vendore ==============================
            var vendor = await userManager.GetUserAsync(User);
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == vendor!.Id);
            if (store == null)
            {
                return NotFound(new { message = "This store is not available" });

            }
            if (!vendor!.Store!.IsActive || vendor.Store.IsDeleted || vendor.Store.Status != StoreStatus.Approved)
            {
                return NotFound(new { message = "This store is not available" });
            }
            // ==========================================

            // Create Product ===========================
            var product = new Product
            {
                Name        = request.Name,
                StoreId     = vendor!.Store!.Id,
                CategoryId  = request.CategoryId,
                BrandId     = request.BrandId,
                Description = request.Description!,
                Price       = request.Price,
                Stock       = request.Stock,
            };

            // Save uploaded image to wwwroot/img 
            if (request.ImageUrl is not null)
            {

                var newFile = await SaveImageAsync(request.ImageUrl);
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img/store", product.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                product.ImageUrl = newFile;
            }

            await ProductRepo.AddAsync(product);
            await ProductRepo.CommitAsync();
            // ==========================================

            return Ok(new { message = "Product Created succesfuly !" });
        }
        #endregion

        #region Get All Product
        [HttpGet]
        public async Task<IActionResult> GetAllProduct([FromQuery] ProductFilterRequest request)
        {
            // Get User ===============================================================================
            var vendor = await userManager.GetUserAsync(User);
            var store  = await StoreRepo.GetOneAsync(e => e.ApplicationUserId == vendor!.Id && e.IsDeleted == false);

            if (store == null)
            {
                return NotFound(new { message = "This store is not available" });

            }
            if (!store.IsActive || store.IsDeleted || store.Status != StoreStatus.Approved)
            {
                return NotFound(new { message = "This store is not available" });
            }
            // ========================================================================================

            // Get All Product ========================================================================
            var products = await ProductRepo.GetAsync(e => e.StoreId == store.Id && e.IsDeleted == false);
            // ========================================================================================

            // Filters ================================================================================
            if (!string.IsNullOrEmpty(request.search))
            {
                products = products.Where(e => e.Name.Contains(request.search, StringComparison.OrdinalIgnoreCase) || ( e.Description != null && e.Description.Contains(request.search, StringComparison.OrdinalIgnoreCase) )).ToList();
            }
            if (request.maxPrice > 1)
            {
                products = products.Where(e => e.Price < request.maxPrice).ToList();
            }
            if (request.minPrice > 1)
            {
                products = products.Where(e => e.Price > request.minPrice).ToList();
            }

            products = products.Where(e => e.IsActive == request.isActive).ToList();
            products = products.Where(e => request.inStock ? e.Stock >= 1 : e.Stock < 1).ToList();

            // ========================================================================================

            // Sorting ================================================================================
            if (request.sortByPrice)
            {
                if (request.descending)
                {
                    products = products.OrderByDescending(e => e.Price).ToList();
                }
                else
                    products = products.OrderBy(e => e.Price).ToList();
            }
            else if (request.sortByDate)
            {
                if (request.descending)
                {
                    products = products.OrderByDescending(e => e.CreatedAt).ToList();
                }
                else
                    products = products.OrderBy(e => e.CreatedAt).ToList();
            }
            else if (request.sortByStock)
            {
                if (request.descending)
                {
                    products = products.OrderByDescending(e => e.Stock).ToList();
                }
                else
                    products = products.OrderBy(e => e.Stock).ToList();
            }

            else if (request.sortBySales)
            {
                if (request.descending)
                {
                    products = products.OrderByDescending(e => e.OrderItems.Where(j => j.ProductId == e.Id).Sum(x => x.Quantity)).ToList();

                }
                else
                    products = products.OrderBy(e => e.OrderItems.Where(j => j.ProductId == e.Id).Sum(x => x.Quantity)).ToList();
            }

            // ========================================================================================

            // Paggination ============================================================================
            products = products.Skip(( request.page - 1 ) * request.pageSize).Take(request.pageSize).ToList();
            // ========================================================================================

            // Mapping ================================================================================
            var result = products.Select(e => new ProductResponse
            {
                Id          = e.Id,
                Name        = e.Name,
                StoreId     = e.StoreId,
                CategoryId  = e.CategoryId,
                Description = e.Description,
                Price       = e.Price,
                Stock       = e.Stock,
                ImageUrl    = e.ImageUrl,
                IsActive    = e.IsActive,
                CreatedAt   = e.CreatedAt
            });
            // ========================================================================================
            return Ok(result);
        }
        #endregion

        #region Get Specific Product
        [HttpGet("{Id:guid}")]
        public async Task<IActionResult> GetSpecificProduct([FromRoute] Guid Id)
        {
            // Get Product ============================================================================================================================
            var user  = await userManager.GetUserAsync(User);
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && e.IsDeleted == false);
            if (store == null)
            {
                return NotFound(new { message = "Store not found" });
            }

            var product = await ProductRepo.GetOneAsync(e=>e.Id == Id && e.IsDeleted == false && e.StoreId == store!.Id);


            if (product == null)
            {
                return NotFound(new { message = "This product is not exist" });
            }
            //==========================================================================================================================================

            // Get Revenue generated ===================================================================================================================
            var revenue         = await OrderItemRepo.GetAsync(e=>e.ProductId == Id);
            var totalSold       = revenue.ToList().Sum(e=>e.Quantity);
            var totalRevenue    = revenue.ToList().Sum(e=>e.Price*e.Quantity);
            //==========================================================================================================================================

            // Get Rate ================================================================================================================================
            var averageRating = (await ReviewRepo.GetAsync(e=>e.ProductId == Id)).ToList().DefaultIfEmpty().Average(r=> r!=null ? r.Rating : 0);
            var totalRatings = (await ReviewRepo.GetAsync(e=>e.ProductId == Id)).ToList().DefaultIfEmpty().Count();
            //==========================================================================================================================================

            // Get Sales trend (last 7/30 days) ========================================================================================================
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var totalSales7Days  = (await OrderItemRepo.GetAsync(e=>e.ProductId == Id && e.CreatedAt >= sevenDaysAgo)).ToList().Sum(i=>i.Quantity);
            var totalRevenue7Days  = (await OrderItemRepo.GetAsync(e=>e.ProductId == Id && e.CreatedAt >= sevenDaysAgo)).ToList().Sum(i=>i.Price * i.Quantity);

            //==========================================================================================================================================


            // Mapping =================================================================================================================================
            var res = new ProductResponse
            {
                Id                    = product.Id,
                Name                  = product.Name,
                StoreId               = product.StoreId,
                CategoryId            = product.CategoryId,
                Description           = product.Description,
                Price                 = product.Price,
                Stock                 = product.Stock,
                ImageUrl              = product.ImageUrl,
                IsActive              = product.IsActive,
                CreatedAt             = product.CreatedAt,
                TotalSold             = totalSold,
                TotalRevenue          = totalRevenue,
                TotalRatings          = totalRatings,
                AverageRating         = averageRating,
                TotalSales7Days       = totalSales7Days,
                TotalRevenue7Days     = totalRevenue7Days,
            };
            //==========================================================================================================================================

            return Ok(res);
        }
        #endregion

        #region Update Product
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] ProductsUpdateRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && e.IsDeleted == false);
            var product = await ProductRepo.GetOneAsync(e=>e.Id == id && e.StoreId == store!.Id && e.IsDeleted == false);
            if (product == null)
                return NotFound(new { Message = "Product not found" });

            // Update basic fields if provided
            if (request.Name != null)
                product.Name = request.Name;

            if (request.Description != null)
                product.Description = request.Description;

            if (request.Price.HasValue)
                product.Price = request.Price.Value;

            if (request.Stock.HasValue)
                product.Stock = request.Stock.Value;

            if (request.CategoryId.HasValue)
                product.CategoryId = request.CategoryId.Value;

            if (request.BrandId.HasValue)
                product.BrandId = request.BrandId.Value;

            if (request.ImageUrl is not null)
            {

                var newFile = await SaveImageAsync(request.ImageUrl);
                if (!string.IsNullOrEmpty(product.ImageUrl) )
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", product.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                product.ImageUrl = newFile;
            }
            await ProductRepo.CommitAsync();

            return Ok(new
            {
                Message = "Product updated successfully",
            });
        }

        #endregion

        #region Delete Product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var user    = await userManager.GetUserAsync(User);
            var store   = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && e.IsDeleted == false);
            if (store is null)
            {
                return BadRequest(new {message = "You are not have a store"});
            }

            var product = await ProductRepo.GetOneAsync(e => e.Id == id);
            if (product!.IsDeleted == true)
            {
                return BadRequest(new {message = "This product is already deleted"});
            }
            product.IsDeleted = true;
            product.StoreId = null;
            await ProductRepo.CommitAsync();
            return Ok(new {message = "The product is deleted succesfully"});
        }
        #endregion

        #region Toggle Product Availability
        [HttpPatch("Availability/{id}")]
        public async Task<IActionResult> ProductAvailability(Guid id)
        {
            var user    = await userManager.GetUserAsync(User);
            var store   = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && e.IsDeleted == false);
            if (store is null)
            {
                return BadRequest(new { message = "You are not have a store" });
            }
            var product = await ProductRepo.GetOneAsync(e => e.Id == id&&e.IsDeleted == false);
            if (product == null)
            {
                return BadRequest(new{message="This product is not exist"});
            }
            product.IsActive = product.IsActive ? false : true;
            await ProductRepo.CommitAsync();
            return Ok();
        } 
        #endregion

        #region Helper

        // Save image in wwwroot/img folder -------------------------------------------------------
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img/store");
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
