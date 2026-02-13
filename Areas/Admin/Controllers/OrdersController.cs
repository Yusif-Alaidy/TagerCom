using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TagerCom.Areas.Admin.DTOs.Responses;
using TagerCom.Areas.Admin.DTOs.Requests;
using TagerCom.Areas.Store.DTOs.Request;
using TagerCom.Areas.Store.DTOs.Response;
using Microsoft.AspNetCore.Authorization;

namespace TagerCom.Areas.Admin.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : ControllerBase
    {
        #region Fiels
        public IRepository<Order> OrderRepo { get; }
        #endregion

        #region Constructore
        public OrdersController (IRepository<Order> OrderRepo)
        {
            this.OrderRepo = OrderRepo;
        }

        #endregion

        #region Get all ordrs
        [HttpGet]
        public async Task<IActionResult> AdminGetAllOrders([FromQuery]OrderFilter request)
        {
            // Get all orders ===================================
            //===================================================
            var orders = await OrderRepo.GetAsync();
            if (orders == null)
                return NotFound();
            //===================================================

            // Filter ===========================================
            // ==================================================
            if (request.StoreID != null)
            {
                orders = orders.Where(e=>e.StoreId == request.StoreID).ToList();
            }

            if (request.CustomerId != null)
            {
                orders = orders.Where(e=>e.CustomerId == request.CustomerId).ToList();
            }

            if (request.startDate != null)
            {
                orders = orders.Where(e=>e.CreatedAt >= request.startDate).ToList();
            }

            if (request.endDate != null)
            {
                orders = orders.Where(e=>e.CreatedAt <= request.endDate).ToList();
            }

            if (request.orderStatus != null)
            {
                orders = orders.Where(e => e.OrderStatus == request.orderStatus).ToList();
            }
            // ==================================================

            // Sorting ==========================================
            // ==================================================
            orders = orders.OrderBy(e=>e.CreatedAt).ToList();
            // ==================================================

            // pagination =======================================
            // ==================================================
            orders = orders.Skip((request.page - 1) * request.pageSize).Take(request.pageSize).ToList();
            // ==================================================

            // Mapping ==========================================
            // ==================================================
            var ordersCount = orders.Count();
            var orderDTO = orders.Select(e => new OrdersRequest
            {
                Id = e.Id,
                CustomerId = e.CustomerId,
                StoreId = e.StoreId,
                OrderStatus = e.OrderStatus,
                TotalAmount = e.TotalAmount,
                CreatedAt = e.CreatedAt,
            });
            
            // ==================================================


            return Ok(new
            {
                Orders              = orderDTO,
                StoreId             = request.StoreID,
                CustomerId          = request.CustomerId,
                StartDate           = request.startDate,
                EndDate             = request.endDate,
                OrderCount          = ordersCount,
                TotalNumberOfPages  = request.pageSize,
                CurrentPage         = request.page,

            });
        }
        #endregion

        #region Get order by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            // GetById Order ==================================
            // ================================================
            var order = await OrderRepo.GetOneAsync(e => e.Id == id, includes: [equals=>equals.OrderItems]);
            if (order == null)
                return NotFound();
            // ================================================

            // Get Item =======================================
            var Items = order.OrderItems.ToList();
            // ================================================

            // ================================================

            // Mapping ========================================
            var orderDTO = new OrdersRequest 
            {
                Id          = order.Id,
                CustomerId  = order.CustomerId,
                StoreId     = order.StoreId,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                CreatedAt   = order.CreatedAt
            };

            // ================================================
            return Ok(new
            {
                Orders = orderDTO,
                Items = Items,
            });
        }
        #endregion

    }
}
