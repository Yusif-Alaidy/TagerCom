using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TagerCom.Areas.Store.DTOs;
using TagerCom.Models;

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
        #endregion

        #region Constructore
        public ProductsController(UserManager<ApplicationUser> userManager, IRepository<Models.Store> storeRepo, IRepository<Product> productRepo)
        {
            this.userManager = userManager;
            this.StoreRepo = storeRepo;
            this.ProductRepo = productRepo;
        }

        #endregion

        #region Create Product
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm]ProductsRequest request)
        {
            // Get vendore ==============================
            var vendor = await userManager.GetUserAsync(User);
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == vendor!.Id);
            if (store == null)
            {
                return NotFound(new { message = "This store is not available" });

            }
            //if (vendor!.Store!.IsActive || vendor.Store.IsDeleted || vendor.Store.Status != StoreStatus.Approved)
            //{
            //    return NotFound(new {message = "This store is not available"});
            //}
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

            return Ok(new {message = "Product Created succesfuly !"});
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
