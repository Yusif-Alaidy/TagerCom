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

        public OrdersController(UserManager<ApplicationUser> userManager,
            IRepository<Order> orderRepo, IRepository<OrderItem> OrItemRepo)
        {
            _userManager = userManager;
            _orderRepo = orderRepo;
            _oritemRepo = OrItemRepo;
        }


        [HttpGet("GetOrder/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            //  Get User  --------------------------------  
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Pure Projection To DTO  --------------------------------------  
            var order = await _orderRepo.Query()
                .Where(o => o.Id == id && o.ApplicationUserId == user.Id)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    CurrentStatus = o.StatusHistory
                      .OrderByDescending(h => h.ChangedAt)
                       .Select(h => h.Status)
                          .FirstOrDefault() ?? o.Status,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    VendorName = o.Vendor.Name ,
                    Items = o.OrderItems 

                        .Select(i => new OrderItemDTO
                        {
                            ProductId = i.ProductId,
                            ProductName = i.Product.Name,
                            Quantity = i.Quantity,
                            Price = i.Price,
                            ImageUrl = i.Product.ImageUrl,
                            Description = i.Product.Description,

                        }).ToList()
                        
                })
                .FirstOrDefaultAsync();

            //  Check order -------------------------------------- 
            if (order == null)
                return NotFound("Order not found or you don’t have access to it");

            
            if (order.Items == null || !order.Items.Any())
                return BadRequest("This order has no items");

             //if (order.Status != "completed")
             //    return BadRequest("Order is not completed yet");

            return Ok(order);
        }



        [HttpGet("GetMyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            //  Get User  --------------------------------  

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Query by Projection DTO  --------------------------------  

            var orders = await _orderRepo.Query()
                .Where(o => o.ApplicationUserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderResponseDTO
                {
                    Id = o.Id,
                    CurrentStatus = o.StatusHistory
                      .OrderByDescending(h => h.ChangedAt)
                       .Select(h => h.Status)
                          .FirstOrDefault() ?? o.Status,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    VendorName = o.Vendor.Name ?? "Unknown Vendor",
                    Items = o.OrderItems.Select(i => new OrderItemDTO
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name ,
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

            // Client Side Projection -------------------------------------------
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
               var historyList = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDTO //ده Logic لأنك بتبني data structure جديد من Entity.
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
                    Description = i.Product.Description,

                 // Here we add every status changes that happened in the product ------------------
                    StatusHistory = historyList
                }).ToList()
            };

            return Ok(dto);
        }




        [HttpDelete("CancelOrder/{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            // Get user  ------------------------------------------

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Query Client Side  ------------------------------------------

            var order = await _orderRepo.Query()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == id && o.ApplicationUserId == user.Id);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status == "Cancelled")
                return BadRequest("Order is already cancelled");

   
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


        [HttpDelete("CancelOrderItem/{orderId}/{itemId}")]
        public async Task<IActionResult> CancelOrderItem(int orderId, int itemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _orderRepo.Query()
                .Include(o => o.OrderItems)
                //Then include because product exist in orderitem as navigation property
                .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ApplicationUserId == user.Id);

            if (order == null) return NotFound("Order not found");

            var item = order.OrderItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound("Item not found in this order");

           
            // Restock product ------------------------------------------
            if (item.Product != null) 
                item.Product.Stock += item.Quantity;

            // Remove item ------------------------------------------
            order.OrderItems.Remove(item);

            // Update order status ------------------------------------------
            order.Status = order.OrderItems.Any() ? "PartiallyCancelled" : "Cancelled";

            // Add status history ------------------------------------------
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = order.Status,
                ChangedAt = DateTime.UtcNow
            });

            await _orderRepo.CommitAsync();

            return Ok(new { message = "Item cancelled successfully", orderStatus = order.Status });
        }


    }





}










    

  


