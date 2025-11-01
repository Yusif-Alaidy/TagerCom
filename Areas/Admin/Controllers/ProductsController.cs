using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TagerCom.DTOs.Request;

namespace TagerCom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]

    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> _productReppository;

        public ProductsController(IRepository<Product> productReppository)
        {
            _productReppository = productReppository;
        }
        #region Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var products = await _productReppository.GetAsync();
            var productRepsonce = products.Adapt<List<ProductResponse>>();
            return Ok(productRepsonce);
        }
        #endregion 
        
        #region Details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productReppository.GetOneAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product.Adapt<ProductResponse>());
        }
        #endregion 
        
        #region Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest productCreateRequest)
        {
            if (productCreateRequest.ImageUrl is not null && productCreateRequest.ImageUrl.Length > 0)
            {
                var fileName = await SaveImageAsync(productCreateRequest.ImageUrl);
                var product = productCreateRequest.Adapt<Product>();
                product.ImageUrl = fileName;
                await _productReppository.AddAsync(product);
                await _productReppository.CommitAsync();

                return Created();
            }
            return BadRequest();
        }
        //Old Code
        /* 
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest productCreateRequest)
        {
            if (productCreateRequest.ImageUrl is not null && productCreateRequest.ImageUrl.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productCreateRequest.ImageUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\Admin\\Products", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await productCreateRequest.ImageUrl.CopyToAsync(stream);
                }
                var product = productCreateRequest.Adapt<Product>();
                product.ImageUrl = fileName;
                await _productReppository.AddAsync(product);
                await _productReppository.CommitAsync();

                return Created();
            }
            return BadRequest();
        }*/
        #endregion

        #region Edit
        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] ProductUpdateRequest productUpdateRequest)
        {
            var productfound = await _productReppository.GetOneAsync(e => e.Id == id, tracked: false);

            if (productfound is null)
                return NotFound();

            productfound.Name = productUpdateRequest.Name ?? productfound.Name;
            productfound.Description = productUpdateRequest.Description ?? productfound.Description;
            productfound.Price = productUpdateRequest.Price != 0 ? productUpdateRequest.Price : productfound.Price;
            productfound.Stock = productUpdateRequest.Stock != 0 ? productUpdateRequest.Stock : productfound.Stock;
            productfound.CategoryId = productUpdateRequest.CategoryId ?? productfound.CategoryId;
            productfound.IsActive = productUpdateRequest.IsActive;

            if (productUpdateRequest.ImageUrl is not null && productUpdateRequest.ImageUrl.Length>0)
            {   
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productUpdateRequest.ImageUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\Admin\\Products", fileName);

                //save new image
                using (var stream = System.IO.File.Create(filePath))
                {
                    await productUpdateRequest.ImageUrl.CopyToAsync(stream);
                }

                //delete old image
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\Admin\\Products", productfound.ImageUrl);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                //update image url
                productfound.ImageUrl = fileName;
            }
            
            //update product in db
            _productReppository.Update(productfound);
            await _productReppository.CommitAsync();

            return NoContent();
        }
        #endregion

        #region Delete
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var productfound = await _productReppository.GetOneAsync(e => e.Id == id, tracked: false);
            if (productfound is null)
                return NotFound();

            //delete old image
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\Admin\\Products", productfound.ImageUrl);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            //delete product from db
            _productReppository.Delete(productfound);
            await _productReppository.CommitAsync();

            return NoContent();
        }
        #endregion

        #region Helper

        // Save image in wwwroot/img/Admin/Products folder -------------------------------------------------------
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img","Admin","Products");
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
