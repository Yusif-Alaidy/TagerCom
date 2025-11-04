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
                //product.img
                ImageUrl = $"{Request.Scheme}://{Request.Host}/{product.ImageUrl}",
                
                Description = product.Description,
            };
            #endregion

            #region === Return Success Response ===
            return Ok(new
            {
                msg = "Product created successfully",
                product = response,
                VendorID = vendor.Id,
                vendorName = user.UserName,
            });
            #endregion
        }


        #endregion

        [Authorize(Roles = "Vendor")]
        [HttpGet("MyProducts")]
        public async Task<IActionResult> GetMyProducts(int? categoryId = null, int? subCategoryId = null, string? search = null,
                string? sortByPrice = null, bool bestSeller = false, int page = 1)


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

        [Authorize(Roles = "Vendor")]

        [HttpGet("MyProduct/{id}")]
        public async Task<IActionResult> GetMyProduct(int id)
        {
            // Get current vendor
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { msg = "Unauthorized user" });

            var vendor = await vendorRepo.GetOneAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null)
                return BadRequest(new { msg = "Vendor profile not found" });

            // Get product that belongs to this vendor
            var product = await productRepo.GetOneAsync(
                  p => p.Id == id && p.VendorId == vendor.Id,
                           include: [p => p.Reviews]);



            if (product == null)
                return NotFound(new { message = "Product not found or does not belong to you" });

            // Convert to DTO
            var productDto = new ProductDetailDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Reviews = product.Reviews.Select(r => new ReviewDTO
                {
                    Comment = r.Comment
                }).ToList()
            };

            return Ok(productDto);
        }

        [Authorize(Roles = "Vendor")]
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDTO dto)
        {
            // ✅ الحصول على الـ Vendor الحالي
            var user = await userManager.GetUserAsync(base.User);
            if (user == null)
                return Unauthorized(new { message = "Unauthorized vendor" });

            var vendor = await vendorRepo.GetOneAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null)
                return BadRequest(new { message = "Vendor profile not found" });

            // ✅ التأكد إن المنتج موجود ويتبع نفس الفيندور
            var product = await productRepo.GetOneAsync(p => p.Id == id && p.VendorId == vendor.Id);
            if (product == null)
                return NotFound(new { message = "Product not found or not owned by you" });

            // ✅ التحقق من Category و SubCategory
            var category = await categoryRepo.GetOneAsync(c => c.Id == dto.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Invalid category" });

            var subCategory = await subCategoryRepo.GetOneAsync(s => s.Id == dto.SubCategoryId);
            if (subCategory == null)
                return BadRequest(new { message = "Invalid subcategory" });

            // ✅ تحديث الحقول
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.SubCategoryId = dto.SubCategoryId;

            // ✅ التعامل مع الصورة الجديدة
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);

                // حذف الصورة القديمة
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // رفع الصورة الجديدة
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var filePath = Path.Combine(rootPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                // المسار النسبي
                product.ImageUrl = Path.Combine("images", "products", uniqueFileName).Replace("\\", "/");
            }

            // ✅ حفظ التعديلات
            productRepo.Update(product);
            await productRepo.CommitAsync();

            // ✅ استخدام DTO للـ response
            var response = new ProductUpdateResponseDTO
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                VendorId = user.Id,
                VendorName = user.UserName
            };

            return Ok(new
            {
                message = "Product updated successfully",
                data = response
            });
        }
        [Authorize(Roles = "Vendor")]
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            #region === Get Current Vendor ===
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Unauthorized user" });

            var vendor = await vendorRepo.GetOneAsync(v => v.ApplicationUserId == user.Id);
            if (vendor == null)
                return BadRequest(new { message = "Vendor profile not found" });
            #endregion

            #region === Find Product ===
            var product = await productRepo.GetOneAsync(p => p.Id == id && p.VendorId == vendor.Id);
            if (product == null)
                return NotFound(new { message = "Product not found or not owned by you" });
            #endregion

            #region === Delete Product Image ===
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                try
                {
                   
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.Replace("/", "\\"));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting image: {ex.Message}");
                }
            }
            #endregion

            #region === Delete Product from Database ===
            productRepo.Delete(product);
            await productRepo.CommitAsync();
            #endregion

            return Ok(new
            {
                message = "Product deleted successfully",
                deletedProductId = id
            });
        }
    }
    }
