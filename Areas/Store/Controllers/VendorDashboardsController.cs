using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TagerCom.Models;
using TagerCom.DTOs;
using TagerCom.Repositories.IRepositories;

namespace TagerCom.Controllers
{
    [ApiController]
    [Route("api/store/[controller]")]
    public class ProductsController : ControllerBase
    {
        #region === Dependencies ===
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepository<Product> productRepo;
        private readonly IRepository<Category> categoryRepo;
        private readonly IRepository<SubCategory> subCategoryRepo;
        #endregion

        #region === Constructor ===
        public ProductsController(
            UserManager<ApplicationUser> userManager,
            IRepository<Product> productRepo,
            IRepository<Category> categoryRepo,
            IRepository<SubCategory> subCategoryRepo)
        {
            this.userManager = userManager;
            this.productRepo = productRepo;
            this.categoryRepo = categoryRepo;
            this.subCategoryRepo = subCategoryRepo;
        }
        #endregion

        #region === Endpoints ===

        /// <summary>
        /// Allows an authenticated Vendor to create a new product.
        /// Validates category, subcategory, and image upload before saving.
        /// </summary>
        [Authorize(Roles = "Vendor")]
        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDTO createProduct)
        {
            #region === Validate Model State ===
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            #endregion

            #region === Get Current Vendor ===
            var vendor = await userManager.GetUserAsync(User);
            if (vendor == null)
                return Unauthorized(new { msg = "Unauthorized vendor" });
            #endregion

            #region === Validate Category & SubCategory ===
            var category = await categoryRepo.GetOneAsync(c => c.Id == createProduct.CategoryId);
            var subCategory = await subCategoryRepo.GetOneAsync(s => s.Id == createProduct.SubCategoryId);

            if (category == null || subCategory == null)
                return BadRequest(new { msg = "Invalid Category or SubCategory" });

            if (subCategory.CategoryId != category.Id)
                return BadRequest(new { msg = "SubCategory does not belong to selected Category" });
            #endregion

            #region === Validate & Save Product Image ===
            if (createProduct.MainImg == null)
                return BadRequest(new { msg = "Product image is required" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(createProduct.MainImg.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { msg = "Invalid image format" });

            // Ensure image folder exists
            var folderPath = Path.Combine("wwwroot", "images", "products");
            Directory.CreateDirectory(folderPath);

            // Generate unique file name and save the image
            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await createProduct.MainImg.CopyToAsync(stream);
            }

            // Build absolute image URL
            string relativePath = $"images/products/{fileName}";
            string baseUrl = $"{Request.Scheme}://{Request.Host}/";
            string imageUrl = baseUrl + relativePath;
            #endregion

            #region === Create Product Entity ===
            var product = createProduct.Adapt<Product>();
            product.ImageUrl = imageUrl;
            product.VendorId = vendor.Id; // VendorId must match ApplicationUser Id type
            product.CreatedAt = DateTime.UtcNow;

            await productRepo.AddAsync(product);
            await productRepo.CommitAsync();
            #endregion

            #region === Prepare Response DTO ===
            var response = new ProductResponseDTO
            {
                Id = product.Id,               // From newly created product
                Name = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                VendorName = vendor.UserName,
                Description = product.Description,
                VendorID = vendor.Id,
            };
            #endregion

            #region === Return Success Response ===
            return Ok(new
            {
                msg = "Product created successfully",
                product = response
            });
            #endregion
        }

        #endregion
    }
}
