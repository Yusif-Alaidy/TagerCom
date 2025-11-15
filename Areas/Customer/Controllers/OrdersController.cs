using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TagerCom.Areas.Customer.DTOs.Request;
using TagerCom.Areas.Customer.DTOs.Response;

namespace TagerCom.Areas.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class OrdersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<OrderItem> _oritemRepo;

        public OrdersController(UserManager<ApplicationUser> userManager, IRepository<Order> orderRepo, IRepository<OrderItem> OrItemRepo)
        {
            _userManager = userManager;
            _orderRepo = orderRepo;
            _oritemRepo = OrItemRepo;

        }


        [HttpGet("GetOrder")]
        public async Task<IActionResult> GetOrder(int id)
        {
            //  Get User  --------------------------------  
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            //  Projection To DTO  --------------------------------  
            var order = await _orderRepo.Query()
                .Where(o => o.Id == id && o.ApplicationUserId == user.Id)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    VendorName = o.Vendor != null ? o.Vendor.Name : "Unknown Vendor",
                    Items = o.OrderItems != null
                        ? o.OrderItems.Select(i => new OrderItemDTO
                        {
                            ProductId = i.ProductId,
                            ProductName = i.Product != null ? i.Product.Name : "Unknown Product",
                            Quantity = i.Quantity,
                            Price = i.Price,
                            ImageUrl = i.Product.ImageUrl,
                            Description=i.Product.Description,

                        }).ToList()
                        : new List<OrderItemDTO>()
                })
                .FirstOrDefaultAsync();

            //  Check order --------------------------------  
            if (order == null)
                return NotFound("Order not found or you don’t have access to it");

            
            if (order.Items == null || !order.Items.Any())
                return BadRequest("This order has no items");

             if (order.Status != "completed")
                 return BadRequest("Order is not completed yet");

            return Ok(order);
        }



        [HttpGet("GetMyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            //  Get User  --------------------------------  

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            //  Projection To DTO  --------------------------------  

            var orders = await _orderRepo.Query()
                .Where(o => o.ApplicationUserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    VendorName = o.Vendor.Name ?? "Unknown Vendor",
                    Items = o.OrderItems.Select(i => new OrderItemDTO
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name ?? "Unknown Product",
                        Quantity = i.Quantity,
                        Price = i.Price,
                        ImageUrl = i.Product.ImageUrl,
                        Description = i.Product.Description
                    }).ToList()
                })
                .ToListAsync();

            // In Case The customer do not have any orders so it will be empty list 
            var safeOrders = orders ?? new List<OrderResponseDTO>();

            return Ok(safeOrders);
        }


        [HttpGet("TrackOrder/{id}")]
        public async Task<IActionResult> TrackOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Query -------------------------------------------
            var order = await _orderRepo.Query()
                .Where(o => o.Id == id && o.ApplicationUserId == user.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Include(o => o.StatusHistory)
                .Include(o => o.Vendor)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Order not found or you don’t have access to it");

            // Get StatusHistory ------------------------------
            //جاب هنا تاريخ الحالات بتاعت الاوردر يعني اتحشن ولا وصل ولا لسه معمول 
            var historyList = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDTO
                {
                    Status = h.Status,
                    ChangedAt = h.ChangedAt
                })
                .ToList();

            // get DTO prepared
            var dto = new OrderTrackDTO
            {
                // هنا بيجيب اخر حاله حصلت للمنتج 

                Id = order.Id,
                CurrentStatus = order.StatusHistory
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => h.Status)
                    .FirstOrDefault() ?? order.Status,

                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                VendorName = order.Vendor?.Name ?? "Unknown Vendor",

                Items = order.OrderItems.Select(i => new OrderItemDTO
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "Unknown Product",
                    Quantity = i.Quantity,
                    Price = i.Price,
                    ImageUrl = i.Product?.ImageUrl,
                    Description = i.Product?.Description,

                 // Here we add every status changes that happened in the product ------------------
                    StatusHistory = historyList
                }).ToList()
            };

            return Ok(dto);
        }




        [HttpPost("CancelOrder/{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            // Get user  ------------------------------------------

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Query  ------------------------------------------

            var order = await _orderRepo.Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == id && o.ApplicationUserId == user.Id);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status == "Cancelled")
                return BadRequest("Order is already cancelled");

            if (order.Status == "Completed" || order.Status == "Shipped")
                return BadRequest("Delivered orders cannot be cancelled");

            // Changing Status ------------------------------------------
            order.Status = "Cancelled";
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = "Cancelled",
                ChangedAt = DateTime.UtcNow
            });

            //  Restock Products  ------------------------------------------

            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                    item.Product.Stock += item.Quantity;
            }

            await _orderRepo.CommitAsync();

            return Ok(new { message = "Order cancelled successfully" });
        }




    }





}










    

  


