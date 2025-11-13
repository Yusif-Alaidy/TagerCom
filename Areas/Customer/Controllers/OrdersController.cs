using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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












    }

    }


