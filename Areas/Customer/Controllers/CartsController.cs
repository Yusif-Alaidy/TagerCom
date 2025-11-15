using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TagerCom.Areas.Customer.Controllers
{
    [Route("api/customer/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class CartsController : ControllerBase
    {

        private readonly IRepository<Cart> _cartRepo;
        private readonly IRepository<CartItem> _cartItemRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Coupon> _couponRepo;
        private readonly IRepository<Points> _pointsRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<OrderItem> _orderItemRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly ApplicationDbContext _dbContext;

        public CartsController(
            IRepository<Cart> cartRepo,
            IRepository<CartItem> cartItemRepo,
            IRepository<Product> productRepo,
            IRepository<Coupon> couponRepo,
            IRepository<Points> pointsRepo,
            IRepository<Order> orderRepo,
            IRepository<OrderItem> orderItemRepo,
            IRepository<Payment> paymentRepo,
            ApplicationDbContext dbContext
            )
        {
            _cartRepo = cartRepo;
            _cartItemRepo = cartItemRepo;
            _productRepo = productRepo;
            _couponRepo = couponRepo;
            _pointsRepo = pointsRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _paymentRepo = paymentRepo;
            _dbContext = dbContext;
        }
        #region AddToCart
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(string userId, int productId, int quantity = 1)
        {
            // Get user's cart
            var cart = (await _cartRepo.GetAsync(c => c.ApplicationUserId == userId, null)).FirstOrDefault();

            if (cart is null)
            {
                cart = new Cart { ApplicationUserId = userId };
                await _cartRepo.AddAsync(cart);
                await _cartRepo.CommitAsync();
            }

            // Get product
            var product = (await _productRepo.GetAsync(p => p.Id == productId, null)).FirstOrDefault();

            if (product is null || !product.IsActive)
                return NotFound(new { msg = "Product not found or inactive" });

            if (product.Stock < quantity)
                return BadRequest(new { msg = "Insufficient stock" });

            // Check if cart item exists
            var cartItem = (await _cartItemRepo.GetAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId, null)).FirstOrDefault();

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                _cartItemRepo.Update(cartItem);
            }
            else
            {
                await _cartItemRepo.AddAsync(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _cartItemRepo.CommitAsync();

            return Ok(new { msg = "Item added to cart successfully" });
        }
        #endregion
        
        #region UpdateCartItem
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            var cartItem = await _cartItemRepo.GetOneAsync(ci => ci.Id == cartItemId, new Expression<Func<CartItem, object>>[] { ci => ci.Product });

            if (cartItem is null)
                return NotFound(new { msg = "Cart item not found." });

            // 🧮 If quantity = 0 → remove the item
            if (quantity <= 0)
            {
                _cartItemRepo.Delete(cartItem);
                await _cartItemRepo.CommitAsync();
                return Ok(new { msg = "Item removed from cart." });
            }

            // 🧾 Check stock
            if (cartItem.Product.Stock < quantity)
                return BadRequest(new { msg = $"Not enough stock for {cartItem.Product.Name}." });

            cartItem.Quantity = quantity;
            _cartItemRepo.Update(cartItem);
            await _cartItemRepo.CommitAsync();

            return Ok(new { msg = "Cart item updated successfully.", cartItemId, newQuantity = quantity });
        }
        #endregion
        
        #region RemoveItem
        [HttpDelete("remove/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var cartItem = await _cartItemRepo.GetOneAsync(ci => ci.Id == itemId);

            if (cartItem is null)
                return NotFound(new { msg = "Cart item not found." });

            _cartItemRepo.Delete(cartItem);
            await _cartItemRepo.CommitAsync();

            return Ok(new { msg = "Item removed successfully." });
        }
        #endregion

        #region ApplyCoupon
        [HttpPost("apply-coupon")]
        public async Task<IActionResult> ApplyCoupon(string userId, string couponCode)
        {
            var coupon = await _couponRepo.GetOneAsync(c => c.Code == couponCode && c.IsActive);

            if (coupon is null) return NotFound(new { msg = "Invalid coupon code." });

            if (coupon.ExpirationDate < DateTime.UtcNow)
                return BadRequest(new { msg = "This coupon has expired." });

            if (coupon.TimesUsed >= coupon.UsageLimit)
                return BadRequest(new { msg = "This coupon has reached its usage limit." });

            var cart = await _cartRepo.GetOneAsync(c => c.ApplicationUserId == userId,
                new Expression<Func<Cart, object>>[] { c => c.CartItems });

            if (cart is null) return NotFound(new { msg = "Cart not found." });

            var cartItems = await _cartItemRepo.GetAsync(ci => ci.CartId == cart.Id,
                new Expression<Func<CartItem, object>>[] { ci => ci.Product });

            if (!cartItems.Any()) return BadRequest(new { msg = "Cart is empty." });

            

            decimal total = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);

            decimal discount = total * (coupon.DiscountPercentage / 100m);
            decimal totalAfterDiscount = total - discount;

            // update coupon usage
            coupon.TimesUsed++;
            _couponRepo.Update(coupon);
            await _couponRepo.CommitAsync();

            return Ok(new
            {
                msg = "Coupon applied successfully.",
                coupon = coupon.Code,
                discountPercentage = coupon.DiscountPercentage,
                totalBefore = total,
                totalAfter = totalAfterDiscount,
                discountAmount = discount
            });
        }

        #endregion

        #region AddPointsToUserAsync
        private async Task AddPointsToUserAsync(string userId, decimal orderTotal)
        {
            // Convert to points
            int earnedPoints = (int)Math.Floor(orderTotal * 0.0005m);

            if (earnedPoints <= 0)
                return;

            // Find existing points record
            var points = await _pointsRepo.GetOneAsync(p => p.ApplicationUserId == userId);

            if (points == null)
            {
                // Create new record
                points = new Points
                {
                    ApplicationUserId = userId,
                    TotalPoints = earnedPoints
                };
                await _pointsRepo.AddAsync(points);
            }
            else
            {
                // Add new points
                points.TotalPoints += earnedPoints;
                points.LastUpdated = DateTime.UtcNow;
                _pointsRepo.Update(points);
            }

            await _pointsRepo.CommitAsync();
        }
        #endregion

        #region ApplyPointsDiscountAsync
        public static class PointsSettings
        {
            public const decimal CurrencyPerPoint = 0.10m; // 1 point = $0.10
        }
        private async Task<decimal> ApplyPointsDiscountAsync(string userId, int pointsRequested)
        {
            var points = await _pointsRepo.GetOneAsync(p => p.ApplicationUserId == userId);

            if (points == null || points.TotalPoints == 0)
                return 0;

            // Ensure user can't use more than they own
            int pointsToUse = Math.Min(pointsRequested, points.TotalPoints);

            // Convert points → money
            decimal discount = pointsToUse * PointsSettings.CurrencyPerPoint;

            // Deduct used points
            points.TotalPoints -= pointsToUse;
            points.LastUpdated = DateTime.UtcNow;

            _pointsRepo.Update(points);
            await _pointsRepo.CommitAsync();

            return discount;
        }
        #endregion

        #region Checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            // get current user id from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("User not logged in.");

            // Load cart with items and product details
            var cart = await _cartRepo.GetOneAsync(c => c.ApplicationUserId == userId,
                new Expression<Func<Cart, object>>[] { c => c.CartItems });

            if (cart == null) return BadRequest("Cart not found or empty.");

            var cartItems = await _cartItemRepo.GetAsync(ci => ci.CartId == cart.Id,
                new Expression<Func<CartItem, object>>[] { ci => ci.Product });

            if (!cartItems.Any()) return BadRequest("Cart has no items.");

            // Validate stock and compute total
            decimal cartTotal = 0;
            foreach (var ci in cartItems)
            {
                if (ci.Product == null) // defensive
                    return BadRequest($"Product (id {ci.ProductId}) not found.");

                if (ci.Product.Stock < ci.Quantity)
                    return BadRequest($"Insufficient stock for product: {ci.Product.Name}");

                cartTotal += ci.Product.Price * ci.Quantity;
            }

            // Apply coupon if provided
            decimal couponDiscount = 0;
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await _couponRepo.GetOneAsync(c => c.Code == request.CouponCode && c.IsActive);
                if (coupon == null)
                    return BadRequest("Invalid coupon code.");

                if (coupon.ExpirationDate < DateTime.UtcNow)
                    return BadRequest("Coupon expired.");

                if (coupon.TimesUsed >= coupon.UsageLimit)
                    return BadRequest("Coupon usage limit reached.");

                // optional vendor filtering
                if (coupon.VendorId.HasValue && !cartItems.Any(ci => ci.Product.VendorId == coupon.VendorId.Value))
                    return BadRequest("Coupon not applicable to cart items.");

                couponDiscount = cartTotal * (coupon.DiscountPercentage / 100m);

                // update coupon usage now (will save inside transaction)
                coupon.TimesUsed++;
                _couponRepo.Update(coupon);
            }

            // Apply points discount if requested
            decimal pointsDiscount = await ApplyPointsDiscountAsync(userId, request.PointsToUse);

            decimal finalTotal = cartTotal - couponDiscount - pointsDiscount;
            if (finalTotal < 0) finalTotal = 0;

            // Start transaction to ensure atomicity
            using (var trx = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Deduct stock
                    foreach (var ci in cartItems)
                    {
                        ci.Product.Stock -= ci.Quantity;
                        _productRepo.Update(ci.Product);
                    }
                    await _productRepo.CommitAsync();

                    // Create Order
                    var order = new Order
                    {
                        ApplicationUserId = userId,
                        VendorId = cartItems.First().Product.VendorId,
                        Status = "Pending",
                        TotalAmount = finalTotal,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _orderRepo.AddAsync(order);
                    await _orderRepo.CommitAsync(); // to get order.Id

                    // Create order items
                    foreach (var ci in cartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = ci.ProductId,
                            Quantity = ci.Quantity,
                            Price = ci.Product.Price
                        };
                        await _orderItemRepo.AddAsync(orderItem);
                    }
                    await _orderItemRepo.CommitAsync();

                    // Create Payment record
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        Method = request.PaymentMethod ?? "Cash",
                        Amount = finalTotal,
                        Status = request.PaymentMethod == "Cash" ? "Pending" : "Paid",
                        TransactionId = null
                    };
                    await _paymentRepo.AddAsync(payment);
                    await _paymentRepo.CommitAsync();

                    // Clear cart items
                    await _cartItemRepo.DeleteRangeAsync(cartItems.ToList());
                    await _cartItemRepo.CommitAsync();

                    // Add reward points (call your method)
                    await AddPointsToUserAsync(userId, order.TotalAmount);

                    await trx.CommitAsync();

                    // Optionally send confirmation here (email/notification)

                    return Ok(new
                    {
                        Message = "Checkout completed successfully",
                        OrderId = order.Id,
                        TotalBefore = cartTotal,
                        CouponDiscount = couponDiscount,
                        PointsDiscount = pointsDiscount,
                        FinalTotal = finalTotal,
                        PaymentMethod = request.PaymentMethod
                    });
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync();
                    // Optionally log ex
                    return StatusCode(500, new { msg = "Checkout failed", error = ex.Message });
                }
            }
        }

        #endregion


    }
}
