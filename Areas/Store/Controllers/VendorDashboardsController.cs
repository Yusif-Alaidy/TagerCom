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
        private readonly IRepository<Vendor> vendorRepo;

        #endregion

        #region === Constructor ===
        public ProductsController(

            UserManager<ApplicationUser> userManager,
            IRepository<Product> productRepo,
            IRepository<Category> categoryRepo,
            IRepository<SubCategory> subCategoryRepo, IRepository<Vendor> vendorRepo)
        {
            this.userManager = userManager;
            this.productRepo = productRepo;
            this.categoryRepo = categoryRepo;
            this.subCategoryRepo = subCategoryRepo;
            this.vendorRepo = vendorRepo;

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
            #region === Get Current Vendor ===
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Unauthorized user" });

            // Get vendor that belongs to this user
            var vendor = await vendorRepo.GetOneAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null)
                return BadRequest(new { message = "Vendor profile not found" });
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
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await createProduct.MainImg.CopyToAsync(stream);
            }

            // Build absolute image URL
            string baseUrl = $"{Request.Scheme}://{Request.Host}/";
            string imageUrl = baseUrl + $"images/products/{fileName}";
            #endregion

            #region === Create Product Entity ===
            var product = createProduct.Adapt<Product>();

            product.ImageUrl = imageUrl;
            product.CategoryId = category.Id;
            product.SubCategoryId = subCategory.Id;

            // ✅ تأكد إن VendorId بياخد Guid صح
            product.VendorId = vendor.Id;

            product.CreatedAt = DateTime.UtcNow;

            await productRepo.AddAsync(product);
            await productRepo.CommitAsync();
            #endregion

            #region === Prepare Response DTO ===
            var response = new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                VendorName = user.UserName, // ✅ لأن Vendor.UserName مش موجود غالبًا
                Description = product.Description,
                VendorID = vendor.Id
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

        [Authorize(Roles = "Vendor")]
        [HttpGet("MyProducts")]
        public async Task<IActionResult> GetMyProducts(
    int? categoryId = null,
    int? subCategoryId = null,
    string? search = null,
    string? sortByPrice = null,
    bool bestSeller = false,
    int page = 1)
        {
            const int pageSize = 10;

            #region === Get Current Vendor ===
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { msg = "Unauthorized user" });

            // ✅ نجيب Vendor اللي بينتمي للمستخدم الحالي
            var vendor = await vendorRepo.GetOneAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null)
                return BadRequest(new { msg = "Vendor profile not found" });
            #endregion

            #region === Base Query ===
            var query = productRepo.Query()
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Vendor)
                .Where(p => p.VendorId == vendor.Id) // ✅ نستخدم الـ Guid الخاص بالـ Vendor
                .AsQueryable();
            #endregion

            #region === Apply Filters ===
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (subCategoryId.HasValue)
                query = query.Where(p => p.SubCategoryId == subCategoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));
            #endregion

            #region === Apply Sorting ===
            if (bestSeller)
                query = query.OrderByDescending(p => p.SalesCount);
            else if (!string.IsNullOrEmpty(sortByPrice))
            {
                query = sortByPrice.ToLower() switch
                {
                    "asc" => query.OrderBy(p => p.Price),
                    "desc" => query.OrderByDescending(p => p.Price),
                    _ => query
                };
            }
            #endregion

            #region === Apply Pagination ===
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            #endregion

            #region === Map to DTO ===
            var productDTOs = products.Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                VendorName = user.UserName, // ✅ نستخدم اسم المستخدم من ApplicationUser
                VendorID = vendor.Id
            }).ToList();
            #endregion

            #region === Return Response ===
            return Ok(new
            {
                msg = "Products retrieved successfully",
                vendor = new
                {
                    vendorId = vendor.Id,
                    username = user.UserName,
                    companyName = vendor.CompanyName
                },
                pagination = new
                {
                    currentPage = page,
                    totalPages,
                    totalItems,
                    pageSize
                },
                products = productDTOs
            });
            #endregion
        }

        
    }
}
