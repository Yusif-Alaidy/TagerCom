using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TagerCom.Areas.Customer.Controllers
{
    [Route("api/customer/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class CartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🧾 GET: api/carts/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var cart = await _context.Cart
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cart == null)
                return NotFound(new { msg = "Cart not found for this user" });

            return Ok(cart);
        }

        // 🛒 POST: api/carts/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(string userId, int productId, int quantity = 1)
        {
            // Find or create user's cart
            var cart = await _context.Cart
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cart == null)
            {
                cart = new Cart { ApplicationUserId = userId };
                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check product exists and stock
            var product = await _context.Product.FindAsync(productId);
            if (product == null || !product.IsActive)
                return NotFound(new { msg = "Product not found or inactive" });

            if (product.Stock < quantity)
                return BadRequest(new { msg = "Not enough stock available" });

            // Check if product already exists in cart
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { msg = "Item added to cart successfully" });
        }

        // ✏️ PUT: api/carts/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItem.FindAsync(cartItemId);
            if (cartItem == null)
                return NotFound(new { msg = "Cart item not found" });

            if (quantity <= 0)
            {
                _context.CartItem.Remove(cartItem);
                await _context.SaveChangesAsync();
                return Ok(new { msg = "Item removed from cart" });
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return Ok(new { msg = "Cart item updated successfully" });
        }

        // ❌ DELETE: api/carts/remove/{itemId}
        [HttpDelete("remove/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var cartItem = await _context.CartItem.FindAsync(itemId);
            if (cartItem == null)
                return NotFound(new { msg = "Cart item not found" });

            _context.CartItem.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { msg = "Item removed successfully" });
        }
    }
}
