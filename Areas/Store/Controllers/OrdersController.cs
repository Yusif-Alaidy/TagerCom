using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TagerCom.Areas.Customer.DTOs.Response;
using TagerCom.Areas.Store.DTOs.Request;
using TagerCom.Areas.Store.DTOs.Response;

namespace TagerCom.Areas.Store.Controllers
{
    [Area("Store")]
    [Route("api/store/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendor")]
    public class OrdersController : ControllerBase
    {

        #region Fields
        public UserManager<ApplicationUser> UserManager { get; }
        public IRepository<Models.Store> StoreRepo { get; }
        public IRepository<Order> OrderRepo { get; }
        #endregion

        #region Constructor
        public OrdersController(UserManager<ApplicationUser> UserManager, IRepository<Models.Store> StoreRepo, IRepository<Order> OrderRepo)
        {
            this.UserManager = UserManager;
            this.StoreRepo = StoreRepo;
            this.OrderRepo = OrderRepo;
        }
        #endregion

        #region Get All Order
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery]FilterOrdersRequest request)
        {
            // Get user ==================================================================================================
            // ===========================================================================================================
            var user = await UserManager.GetUserAsync(User);
            // ===========================================================================================================

            // Get store =================================================================================================
            // ===========================================================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive);
            if (store == null)
            {
                return NotFound(new {message = "this store is not exist!"});
            }
            // ===========================================================================================================

            // Get orders ================================================================================================
            // ===========================================================================================================
            var orders = await OrderRepo.GetAsync(e=>e.StoreId == store.Id, includes:[equals=>equals.Customer, ]);
            Console.WriteLine(orders.Count);
            // ===========================================================================================================
            var totalPage = orders.Count() / 10;
            // Filter ====================================================================================================
            // ===========================================================================================================
            if (request.startDate != null)
            {
                orders = orders.Where(e=>e.CreatedAt >= request.startDate).ToList();
            }
            if (request.endDate != null)
            {
                orders = orders.Where(e=>e.CreatedAt <= request.endDate).ToList();
            }
            if(request.orderStatus != null)
            {
                orders = orders.Where(e=>e.OrderStatus == request.orderStatus).ToList();
            }
            if (!string.IsNullOrWhiteSpace(request.customerUsernameOrEmail))
            {
                orders = orders.Where(e => e.Customer.Email == request.customerUsernameOrEmail || e.Customer.UserName == request.customerUsernameOrEmail).ToList();            
            }
            // ===========================================================================================================
            
            // Sort ======================================================================================================
            // ===========================================================================================================
            if (request.newest)
            {
                orders = orders.OrderByDescending(e=>e.CreatedAt).ToList();
            }
            else if (request.oldest)
            {
                orders = orders.OrderBy(e=>e.CreatedAt).ToList();
            }
            // ===========================================================================================================
            orders = orders.Skip(( request.page - 1 ) * request.pageSize).Take(request.pageSize).ToList();
            var ordersDto = orders.Select(e => new OrdersResponse
            {
                Id          = e.Id,
                Customer    = e.Customer.UserName,
                Store       = e.Store!.StoreName,
                OrderStatus = e.OrderStatus,
                TotalAmount = e.TotalAmount,
                CreatedAt   = e.CreatedAt,
            });
            return Ok(new
            {
                TotalPage = totalPage,
                PageSize = request.pageSize,
                Page = request.page,
                ordersDto,
            });
        }
        #endregion
    }
}
