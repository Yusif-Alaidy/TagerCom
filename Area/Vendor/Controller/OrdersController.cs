using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TagerCom.Area.Vendor.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        #region Fields
        private readonly IOrderRepository _orderRepository;
        #endregion

        #region Constructor
        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        #endregion

        #region Endpoints - Orders

        // GET: api/vendor/orders?vendorId=123
        [HttpGet]
        public async Task<IActionResult> GetMyOrders([FromQuery] int vendorId)
        {
            var orders = await _orderRepository.GetAllAsync(
                filter: o => o.VendorId == vendorId,
                include: q => q
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product),
                tracked: false
            );

            if (orders == null || !orders.Any())
                return NotFound("No orders found for this vendor.");

            // Mapster يحول List<Order> → List<TotalOrderSalesDto>
            var response = orders.Adapt<List<TotalOrderSalesDto>>();

            return Ok(response);
        }

        // GET: api/vendor/orders/5?vendorId=123
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOneOrder(int id, [FromQuery] int vendorId)
        {
            var order = await _orderRepository.GetOneAsync(
                filter: o => o.Id == id && o.VendorId == vendorId,
                    include: q => q
                        .Include(o => o.Customer)
                           .Include(o => o.OrderItems)
                                  .ThenInclude(oi => oi.Product),
                                                        tracked: false);


            if (order == null)
                return NotFound("Order not found for this vendor.");

            // هنا نستخدم Adapt مباشرة بدل Select يدوي
            var response = order.Adapt<TotalOrderSalesDto>();

            return Ok(response);

        }


        #endregion

        #region Endpoints - Vendor Sales

        // Endpoint 1: كل مبيعات البائع وصافي الربح
        // GET: api/orders/{vendorId}/sales
        [HttpGet("{vendorId}/sales")]
        public async Task<IActionResult> GetVendorSales(int vendorId)
        {
            var orders = await _orderRepository.GetVendorSalesAsync(vendorId);

            var response = new VendorSalesResponseDto
            {
                VendorId = vendorId,
                TotalSales = orders.Sum(o => o.TotalAmount),
                TotalProfit = orders.Sum(o =>
                    o.OrderItems.Sum(oi => (oi.Price - oi.Product.Price) * oi.Quantity)
                ),
                Orders = orders.Select(o => new TotalOrderSalesDto
                {
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                }).ToList()
            };

            return Ok(response);
        }

        // Endpoint 2: مبيعات البائع مع فلترة زمنية
        // GET: api/vendor/{vendorId}/sales/filter-by-date?startDate=2025-01-01&endDate=2025-01-31
        [HttpGet("{vendorId}/sales/filter-by-date")]
        public async Task<IActionResult> GetVendorSalesByDate(int vendorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var orders = await _orderRepository.GetVendorSalesAsync(vendorId, startDate, endDate);

            var response = new VendorSalesResponseDto
            {
                VendorId = vendorId,
                TotalSales = orders.Sum(o => o.TotalAmount),
                TotalProfit = orders.Sum(o =>
                    o.OrderItems.Sum(oi => (oi.Price - oi.Product.Price) * oi.Quantity)
                ),
                Orders = orders.Select(o => new TotalOrderSalesDto
                {
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                }).ToList()
            };

            return Ok(response);
        }

        #endregion
    }
}