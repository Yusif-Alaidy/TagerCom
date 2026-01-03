using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TagerCom.Areas.Customer.DTOs.Response;
using TagerCom.Areas.Store.DTOs.Request;
using TagerCom.Areas.Store.DTOs.Response;
using TagerCom.Models;
using TagerCom.Utility;

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
        public IEmailSender EmailSender { get; }
        public IRepository<Product> ProductRepo { get; }
        public ApplicationDbContext Context { get; }
        public ILogger<OrdersController> Logger { get; }
        #endregion

        #region Constructor
        public OrdersController(
            UserManager<ApplicationUser> UserManager,
            IRepository<Models.Store> StoreRepo,
            IRepository<Order> OrderRepo,
            IEmailSender EmailSender,
            IRepository<Product> ProductRepo,
            ApplicationDbContext context,
            ILogger<OrdersController> logger
            )
        {
            this.UserManager = UserManager;
            this.StoreRepo = StoreRepo;
            this.OrderRepo = OrderRepo;
            this.EmailSender = EmailSender;
            this.ProductRepo = ProductRepo;
            this.Context = context;
            this.Logger = logger;
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

        #region Get One
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(Guid id)
        {
            // Get user ====================================================================================================
            // =============================================================================================================
            var user = await UserManager.GetUserAsync(User);
            // =============================================================================================================

            // Get store
            // =============================================================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive);
            if (store == null)
            {
                return NotFound(new {message = "This store is not exist"});
            }
            // =============================================================================================================

            // Get order ===================================================================================================
            // =============================================================================================================
            var order = await OrderRepo.GetOneAsync(e=>e.Id == id && e.StoreId == store.Id);
            var orderDto = new OrdersResponse
            {

                Id          = order!.Id,
                Customer    = order.Customer.UserName,
                Store       = order.Store!.StoreName,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                CreatedAt   = order.CreatedAt,
            };
            // =============================================================================================================

            return Ok(orderDto);
        }
        #endregion

        #region Confirm Order
        [HttpPatch("confirm/{id}")]
        public async Task<IActionResult> ConfirmOrder([FromRoute]Guid id)
        {
            // Get user ========================================================
            // =================================================================
            var user = await UserManager.GetUserAsync(User);
            // =================================================================

            // Get store =======================================================
            // =================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive);
            if (store == null)
                return NotFound(new {message = "This store is not founded!"});
            // =================================================================

            // Get order =======================================================
            // =================================================================

            var order = await OrderRepo.Query()
                .Where(e => e.Id == id && e.StoreId == store.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // ✅ Include Product
                .Include(o => o.Customer) // ✅ Include Customer
                .FirstOrDefaultAsync();
            if (order == null)
                return NotFound(new {message = "This order is not founded!"});
        
            var orderStatus = order.OrderStatus == OrderStatus.Pending || order.OrderStatus == OrderStatus.AwaitingPayment? true : false;
            if (!orderStatus)
            {
                return BadRequest(new {message = "This Order is can't confirm"});
            }
            // =================================================================
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                // Get change status ===============================================
                // =================================================================
                order.OrderStatus = OrderStatus.Confirmed;
                await OrderRepo.CommitAsync();
                // =================================================================

                // Change product Qauntity =========================================
                // =================================================================
                var orderItem = order.OrderItems;
                foreach (var item in orderItem)
                {
                    var product = await ProductRepo.GetOneAsync(p => p.Id == item.Product.Id);

                    if (product == null)
                    {
                        return NotFound(new { message = $"Product {item.ProductId} not found" });
                    }

                    // ✅ Check stock availability
                    if (product.Stock < item.Quantity)
                    {
                        return BadRequest(new
                        {
                            message = $"Insufficient stock for {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}"
                        });
                    }

                    product.Stock -= item.Quantity;
                    ProductRepo.Update(product);
                }
                // ✅ Commit مرة واحدة بعد اللووب
                await ProductRepo.CommitAsync();
                // =================================================================

                // Get send email ==================================================
                // =================================================================
                await transaction.CommitAsync();
                await EmailSender.SendEmailAsync(order.Customer.Email!, "Your Order Is Confirmed 🎉", $"Thank you for shopping with us!\r\nYour order is confirmed and is now being prepared for shipment.\r\nYou’ll receive an update as soon as it’s on the way.");
                // =================================================================

                return Ok(new {message = "Order Confirm successfuly. "});
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.LogError(ex, "Error confirming order {OrderId}", id);
                return StatusCode(500, new { message = "Failed to confirm order" });
            }
        }
        #endregion

        #region Change status
        [HttpPatch("change-status/{id}")]
        public async Task<IActionResult> ChangeStatus(Guid id)
        {
            // Get user ==========================================================
            // ===================================================================
            var user = await UserManager.GetUserAsync(User);
            // ===================================================================

            // Get store =========================================================
            // ===================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive);
            if (store == null)
            {
                return NotFound(new {message = "This store is not exist!"});
            }
            // ===================================================================

            // Get Order =========================================================
            // ===================================================================
            var order = await OrderRepo.GetOneAsync(e=>e.Id == id && e.StoreId == store.Id);
            if (order == null)
                return NotFound(new { message = "This order is not founded!" });

            var orderStatus = order.OrderStatus == OrderStatus.Confirmed || 
                order.OrderStatus == OrderStatus.Processing ||
                order.OrderStatus == OrderStatus.ReadyToShip||
                order.OrderStatus == OrderStatus.Shipped    ||
                order.OrderStatus == OrderStatus.OutForDelivery ? false : true;

            if (orderStatus)
            {
                return BadRequest(new { message = "This Order is can't confirm" });
            }
            // ===================================================================

            // Change Status  ====================================================
            // ===================================================================
            order.OrderStatus += 1;
            await OrderRepo.CommitAsync();
            // ===================================================================

            return Ok(new {message = $"Order status changed successfuly. your order status is {order.OrderStatus} now"});
        }
        #endregion

        #region Cancel
        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            // Get user ==========================================================
            // ===================================================================
            var user = await UserManager.GetUserAsync(User);
            // ===================================================================

            // Get store =========================================================
            // ===================================================================
            var store = await StoreRepo.GetOneAsync(e=>e.ApplicationUserId == user!.Id && !e.IsDeleted && e.IsActive);
            if (store == null)
            {
                return NotFound(new { message = "This store is not exist!" });
            }
            // ===================================================================

            // Get Order =========================================================
            // ===================================================================
            //var order = await OrderRepo.GetOneAsync(e=>e.Id == id && e.StoreId == store.Id);
            var order = await OrderRepo.Query()
                .Where(e => e.Id == id && e.StoreId == store.Id)
                .Include(e=>e.OrderItems)
                .ThenInclude(e=>e.Product)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { message = "This order is not founded!" });


            var orderStatus = order.OrderStatus == OrderStatus.Confirmed ||
                order.OrderStatus == OrderStatus.Pending                 ||
                order.OrderStatus == OrderStatus.AwaitingPayment         ||
                order.OrderStatus == OrderStatus.Confirmed               ||
                order.OrderStatus == OrderStatus.Processing              ||
                order.OrderStatus == OrderStatus.ReadyToShip 
                ? false : true;

            if (orderStatus)
            {
                return BadRequest(new { message = "This Order is can't confirm" });
            }
            // ===================================================================

            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {

                // Change Status  ====================================================
                // ===================================================================
                order.OrderStatus = OrderStatus.Cancelled;
                await OrderRepo.CommitAsync();
                // ===================================================================

                // Return product quantity ===========================================
                // ===================================================================
                var orderItems = order.OrderItems;
                foreach (var o in orderItems)
                {
                    o.Product.Stock += o.Quantity;
                    await OrderRepo.CommitAsync();
                }
                // ===================================================================


                await transaction.CommitAsync();
                return Ok(new { message = $"Order is Canceled successfuly. " });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.LogError(ex, "Error confirming order {OrderId}", id);
                return StatusCode(500, new { message = "Failed to confirm order" });
            }
        }
        #endregion


    }
}
