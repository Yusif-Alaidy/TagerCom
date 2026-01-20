using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TagerCom.Areas.Admin.DTOs.Requests;
using TagerCom.Areas.Admin.DTOs.Responses;

namespace TagerCom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class VendorsController : ControllerBase
    {
        #region Fields
        public IRepository<Models.Store> StoreRepo { get; }
        public IRepository<Product> ProductRepo { get; }
        public IRepository<Order> OrderRepo { get; }
        public IRepository<CartItem> CartItemRepo { get; }
        #endregion

        #region Constructore
        public VendorsController(IRepository<Models.Store> StoreRepo, IRepository<Product> ProductRepo, IRepository<Order> OrderRepo, IRepository<CartItem> CartItemRepo)
        {
            this.StoreRepo = StoreRepo;
            this.ProductRepo = ProductRepo;
            this.OrderRepo = OrderRepo;
            this.CartItemRepo = CartItemRepo;
        }

        #endregion

        #region Get All Vendor
        [HttpGet]
        public async Task<IActionResult> GetAllVendors([FromQuery] GetVendorsRequest request)
        {
            // Start from repository query (IQueryable)
            var query = StoreRepo.Query()
        .AsNoTracking()
        .Where(s => !s.IsDeleted);

            // Filters
            if (request.Status != null)
                query = query.Where(s => s.Status == request.Status);

            if (request.IsActive != null)
                query = query.Where(s => s.IsActive == request.IsActive);

            if (request.StartDate != null)
                query = query.Where(s => s.CreatedAt >= request.StartDate);

            if (request.EndDate != null)
                query = query.Where(s => s.CreatedAt <= request.EndDate);

            // Search (Vendor Name / Email / Store Name)
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(s =>
                    s.StoreName.Contains(search) ||
                    s.ApplicationUser.UserName.Contains(search) ||
                    s.ApplicationUser.Email.Contains(search));
            }

            // Projection to compute Sales/Revenue in SQL
            var projected = query.Select(s => new
            {
                Store = s,
                Vendor = s.ApplicationUser,

                TotalSales = s.Products
            .SelectMany(p => p.OrderItems)
            .Sum(oi => (int?)oi.Quantity) ?? 0,

                TotalRevenue = s.Products
            .SelectMany(p => p.OrderItems)
            .Sum(oi => (decimal?)(oi.Quantity * oi.Price)) ?? 0
            });

            // Sorting
            projected = (request.SortBy, request.Desc) switch
            {
                (VendorSortBy.Sales, true) => projected.OrderByDescending(x => x.TotalSales),
                (VendorSortBy.Sales, false) => projected.OrderBy(x => x.TotalSales),

                (VendorSortBy.Revenue, true) => projected.OrderByDescending(x => x.TotalRevenue),
                (VendorSortBy.Revenue, false) => projected.OrderBy(x => x.TotalRevenue),

                (VendorSortBy.Date, true) => projected.OrderByDescending(x => x.Store.CreatedAt),
                _ => projected.OrderBy(x => x.Store.CreatedAt)
            };

            // Total count AFTER filters/search (before pagination)
            var totalCount = await projected.CountAsync();

            // Pagination + mapping
            var items = await projected
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(x => new VendorListItemResponse
        {
            VendorId = x.Vendor.Id,
            VendorName = x.Vendor.UserName,
            VendorEmail = x.Vendor.Email,

            StoreId = x.Store.Id,
            StoreName = x.Store.StoreName,

            Status = x.Store.Status,
            IsActive = x.Store.IsActive,
            RegisteredAt = x.Store.CreatedAt,

            TotalSales = x.TotalSales,
            TotalRevenue = x.TotalRevenue
        })
        .ToListAsync();

            return Ok(new PagedResponse<VendorListItemResponse>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                Items = items
            });
        }
        #endregion

        #region Get Specific Vendor
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetOneVendor(Guid id)
        {
            // Get Vendor ==============================================================
            // -----------
            var vendor = await StoreRepo.GetOneAsync(e=>e.Id == id && !e.IsDeleted && e.IsActive, includes:[e=>e.ApplicationUser!, equals=>equals.Orders]);
            if (vendor == null)
                return NotFound("Vendor not found");
            // =========================================================================

            // Acounting Proccess ======================================================
            // -------------------
            var rating       = (await ProductRepo.GetAsync(e=>e.StoreId == vendor!.Id && !e.IsDeleted ,includes:[equals=>equals.Reviews])).SelectMany(e=>e.Reviews).Average(e => (decimal?)e.Rating) ?? 0;
            var totalReviews = (await ProductRepo.GetAsync(e=>e.StoreId == vendor!.Id && !e.IsDeleted ,includes:[equals=>equals.Reviews])).SelectMany(e=>e.Reviews).Count();
            var totalProduct = (await ProductRepo.GetAsync(e=>e.StoreId == vendor!.Id && !e.IsDeleted)).Count();
            var totalOrder   = (await OrderRepo.GetAsync(e=>e.StoreId == vendor!.Id)).Count();
            var totalSales   = (await OrderRepo.GetAsync(e=>e.StoreId == vendor!.Id)).Average(e=>e.TotalAmount) ?? 0;
            // =========================================================================

            // Mapping =================================================================
            // --------
            var response = new VendorDetailsResponse
            {
                Vendor = new VendorDetails
                {
                    Id           = vendor!.ApplicationUser!.Id,
                    UserName     = $"{vendor.ApplicationUser?.FirstName} {vendor.ApplicationUser?.LastName}".Trim(),
                    Email        = vendor.ApplicationUser!.Email!,
                    PhoneNumber  = vendor.ApplicationUser.PhoneNumbers ?? string.Empty
                },
                Store = new StoreDetails
                {
                    Id              = vendor.Id,
                    StoreName       = vendor.StoreName,
                    Status          = vendor.Status,
                    IsActive        = vendor.IsActive,
                    Rating          = rating,
                    TotalProducts   = totalProduct,
                    TotalOrders     = totalOrder,
                    RegisteredAt    = vendor.CreatedAt,

                },
                Performance = new VendorPerformance
                {
                    TotalSales = totalSales,
                }
            };

            // =========================================================================
            return Ok(response);
        }
        #endregion

        #region Reject Store
        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {

            var vendor = await StoreRepo.GetOneAsync(e=>e.Id == id && e.Status == StoreStatus.Pending);
            if (vendor == null)
                return BadRequest(new {message = "this store is not exist"});

            vendor.Status = StoreStatus.Rejected;
            await StoreRepo.CommitAsync();
            return Ok(new {message = "this store is rejected successfuly"});
        }
        #endregion

        #region Approve Store
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {

            var vendor = await StoreRepo.GetOneAsync(e=>e.Id == id && e.Status == StoreStatus.Pending);
            if (vendor == null)
                return BadRequest(new {message = "this store is not exist"});

            vendor.Status = StoreStatus.Approved;
            vendor.IsActive = true;
            await StoreRepo.CommitAsync();
            return Ok(new {message = "this store is approved successfuly"});
        }
        #endregion

        #region Block Store
        [HttpPatch("{id}/Block")]
        public async Task<IActionResult> Block(Guid id)
        {

            var vendor = await StoreRepo.GetOneAsync(e=>e.Id == id && e.Status == StoreStatus.Approved);
            if (vendor == null)
                return BadRequest(new {message = "this store is not exist"});

            if (vendor.IsActive == false)
                return BadRequest(new {message = "this store is aleardy blocked"});

            vendor.IsActive = false;
            await StoreRepo.CommitAsync();

            return Ok(new {message = "this store is blocked successfuly"});
        }
        #endregion

        #region Unblock Store
        [HttpPatch("{id}/unblock")]
        public async Task<IActionResult> Unblock(Guid id)
        {

            var vendor = await StoreRepo.GetOneAsync(e=>e.Id == id && e.Status == StoreStatus.Approved);
            if (vendor == null)
                return BadRequest(new {message = "this store is not exist"});

            if (vendor.IsActive == true)
                return BadRequest(new { message = "this store is aleardy unblocked"});

            vendor.IsActive = true;
            await StoreRepo.CommitAsync();
            return Ok(new {message = "this store is Unblocked successfuly"});
        }
        #endregion

        #region Delete Store
        [HttpPatch("{id}/delete")]
        public async Task<IActionResult> DeleteStore(Guid id)
        {

            // Get store ==========================================================================================================================
            // ====================================================================================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.Id == id && !e.IsDeleted && e.IsActive, includes:[equals=>equals.Orders]);
            if (store is null)
            {
                return BadRequest(new { message = "This store is not exist!" });
            }
            // ===================================================================================================================================

            // Chech if can delete ===============================================================================================================
            // ===================================================================================================================================
            var hasActiveOrder = store.Orders.Any(e=>
            e.OrderStatus != OrderStatus.Refunded  &&
            e.OrderStatus != OrderStatus.Cancelled &&
            e.OrderStatus != OrderStatus.Completed );
            if (hasActiveOrder)
            {
                return BadRequest(new
                {
                    message = "Cannot delete account. This store have active orders."
                });
            }
            // ===================================================================================================================================


            // Delete Proccess ===================================================================================================================
            // ===================================================================================================================================
            store.IsDeleted = true;
            store.IsActive = false;

            var products = await ProductRepo.GetAsync(equals=>equals.StoreId == store.Id && !equals.IsDeleted);
            var productsIds = products.Select(e => e.Id);

            var cartItems = await CartItemRepo.GetAsync(e=>productsIds.Contains(e.ProductId));

            foreach (var p in products)
            {
                p.IsActive = false;
                p.IsDeleted = true;
            }

            await StoreRepo.CommitAsync();
            // ===================================================================================================================================
            return Ok(new { message = "Delete store successfuly!" });
        }
        #endregion


    }
}
