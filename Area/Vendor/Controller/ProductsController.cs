using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TagerCom.Area.Vendor.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        #region Fields
        private readonly IRepository<Product> _repo;
        #endregion

        #region Constructor
        public ProductsController(IRepository<Product> repo)
        {
            _repo = repo;
        }
        #endregion

        #region Endpoints - Products

        // GET: api/vendor/products?vendorId=123
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int vendorId)
        {
            var products = await _repo.GetAllAsync(
                filter: p => p.VendorId == vendorId,
                include: q => q.Include(p => p.Category),
                tracked: false
            );

            if (products == null || !products.Any())
                return NotFound("No products found for this vendor.");

            return Ok(products);
        }

        // GET: api/vendor/products/5?vendorId=123
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id, [FromQuery] int vendorId)
        {
            var product = await _repo.GetOneAsync(filter: p => p.Id == id && p.VendorId == vendorId,
                  include: q => q.Include(p => p.Category), tracked: false);

            if (product == null)
                return NotFound("Product not found for this vendor.");

            return Ok(product);
        }

        #endregion
    }
}