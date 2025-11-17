using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;
using TagerCom.DataAccess;
using TagerCom.DTOs.Request;
using TagerCom.Models;
using TagerCom.Repositories.IRepositories;

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

        // ----------------------------------------------------
        // Helpers
        // ----------------------------------------------------
        private string? GetUserIdFromClaims()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ----------------------------------------------------
        // Add to cart
        // (still accepts userId param like your original code)
        // ----------------------------------------------------
        #region AddToCart
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(string userId, int productId, int quantity = 1)
        {
            if (quantity <= 0)
                return BadRequest(new { msg = "Quantity must be at least 1." });

            // Get user's cart
            var cart = (await _cartRepo.GetAsync(c => c.ApplicationUserId == userId, null))
                .FirstOrDefault();

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
            var cartItem = (await _cartItemRepo.GetAsync(
                ci => ci.CartId == cart.Id && ci.ProductId == productId, null)).FirstOrDefault();

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

        // ----------------------------------------------------
        // Update cart item
        // ----------------------------------------------------
        #region UpdateCartItem
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            var cartItem = await _cartItemRepo.GetOneAsync(
                ci => ci.Id == cartItemId,
                new Expression<Func<CartItem, object>>[] { ci => ci.Product });

            if (cartItem is null)
                return NotFound(new { msg = "Cart item not found." });

            // If quantity = 0 → remove the item
            if (quantity <= 0)
            {
                _cartItemRepo.Delete(cartItem);
                await _cartItemRepo.CommitAsync();
                return Ok(new { msg = "Item removed from cart." });
            }

            // Check stock
            if (cartItem.Product.Stock < quantity)
                return BadRequest(new { msg = $"Not enough stock for {cartItem.Product.Name}." });

            cartItem.Quantity = quantity;
            _cartItemRepo.Update(cartItem);
            await _cartItemRepo.CommitAsync();

            return Ok(new { msg = "Cart item updated successfully.", cartItemId, newQuantity = quantity });
        }
        #endregion

        // ----------------------------------------------------
        // Remove item
        // ----------------------------------------------------
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

        // ----------------------------------------------------
        // PRIVATE – Apply Coupon (no API endpoint)
        // ----------------------------------------------------
        #region ApplyCouponHelper
        private async Task<decimal> ApplyCouponAsync(string couponCode, decimal cartTotal, List<CartItem> cartItems)
        {
            var coupon = await _couponRepo.GetOneAsync(c => c.Code == couponCode && c.IsActive);

            if (coupon == null)
                throw new Exception("Invalid coupon code.");

            if (coupon.ExpirationDate < DateTime.UtcNow)
                throw new Exception("This coupon has expired.");

            if (coupon.TimesUsed >= coupon.UsageLimit)
                throw new Exception("This coupon has reached its usage limit.");

            // Optional vendor-specific coupon (Coupon.VendorId is Guid?)
            if (coupon.VendorId.HasValue)
            {
                var vendorId = coupon.VendorId.Value;

                if (!cartItems.Any(ci => ci.Product.VendorId == vendorId))
                    throw new Exception("Coupon not applicable to these items.");
            }

            decimal discount = cartTotal * (coupon.DiscountPercentage / 100m);

            // Increase usage (will be saved in the same transaction)
            coupon.TimesUsed++;
            _couponRepo.Update(coupon);

            return discount;
        }
        #endregion

        // ----------------------------------------------------
        // PRIVATE – Add points after order
        // ----------------------------------------------------
        #region AddPointsToUserAsync
        private async Task AddPointsToUserAsync(string userId, decimal orderTotal)
        {
            int earnedPoints = (int)Math.Floor(orderTotal * 0.0005m);
            if (earnedPoints <= 0) return;

            var points = await _pointsRepo.GetOneAsync(p => p.ApplicationUserId == userId);

            if (points == null)
            {
                points = new Points
                {
                    ApplicationUserId = userId,
                    TotalPoints = earnedPoints,
                    LastUpdated = DateTime.UtcNow
                };
                await _pointsRepo.AddAsync(points);
            }
            else
            {
                points.TotalPoints += earnedPoints;
                points.LastUpdated = DateTime.UtcNow;
                _pointsRepo.Update(points);
            }

            await _pointsRepo.CommitAsync();
        }
        #endregion

        // ----------------------------------------------------
        // PRIVATE – Apply points discount (no API endpoint)
        // ----------------------------------------------------
        #region ApplyPointsDiscountAsync
        public static class PointsSettings
        {
            public const decimal CurrencyPerPoint = 0.10m; // 1 point = 0.10
        }

        private async Task<decimal> ApplyPointsDiscountAsync(string userId, int pointsRequested)
        {
            if (pointsRequested <= 0)
                return 0;

            var points = await _pointsRepo.GetOneAsync(p => p.ApplicationUserId == userId);

            if (points == null || points.TotalPoints <= 0)
                return 0;

            int pointsToUse = Math.Min(pointsRequested, points.TotalPoints);
            decimal discount = pointsToUse * PointsSettings.CurrencyPerPoint;

            points.TotalPoints -= pointsToUse;
            points.LastUpdated = DateTime.UtcNow;

            _pointsRepo.Update(points);
            await _pointsRepo.CommitAsync();

            return discount;
        }
        #endregion

        // ----------------------------------------------------
        // Checkout – single atomic operation
        // ----------------------------------------------------
        #region Checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized("User not logged in.");

            // Load cart with items and product
            var cart = await _cartRepo.GetOneAsync(
                c => c.ApplicationUserId == userId,
                new Expression<Func<Cart, object>>[] { c => c.CartItems });

            if (cart == null)
                return BadRequest("Cart not found or empty.");

            var cartItems = await _cartItemRepo.GetAsync(
                ci => ci.CartId == cart.Id,
                new Expression<Func<CartItem, object>>[] { ci => ci.Product });

            if (!cartItems.Any())
                return BadRequest("Cart has no items.");

            // Validate stock & compute total
            decimal cartTotal = 0;
            foreach (var ci in cartItems)
            {
                if (ci.Product == null)
                    return BadRequest($"Product (id {ci.ProductId}) not found.");

                if (ci.Product.Stock < ci.Quantity)
                    return BadRequest($"Insufficient stock for product: {ci.Product.Name}");

                cartTotal += ci.Product.Price * ci.Quantity;
            }

            decimal couponDiscount = 0;
            decimal pointsDiscount = 0;

            using (var trx = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // ----- Coupon -----
                    if (!string.IsNullOrWhiteSpace(request.CouponCode))
                    {
                        try
                        {
                            couponDiscount = await ApplyCouponAsync(
                                request.CouponCode,
                                cartTotal,
                                cartItems.ToList()
                            );
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(new { msg = ex.Message });
                        }
                    }

                    // ----- Points -----
                    pointsDiscount = await ApplyPointsDiscountAsync(userId, request.PointsToUse);

                    // Final total
                    decimal finalTotal = cartTotal - couponDiscount - pointsDiscount;
                    if (finalTotal < 0) finalTotal = 0;

                    // Deduct stock
                    foreach (var ci in cartItems)
                    {
                        ci.Product.Stock -= ci.Quantity;
                        _productRepo.Update(ci.Product);
                    }
                    await _productRepo.CommitAsync();

                    // Create order
                    var order = new Order
                    {
                        ApplicationUserId = userId,
                        VendorId = cartItems.First().Product.VendorId, // Product.VendorId is Guid
                        Status = "Pending",
                        TotalAmount = finalTotal,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _orderRepo.AddAsync(order);
                    await _orderRepo.CommitAsync();

                    // Order items
                    foreach (var ci in cartItems)
                    {
                        await _orderItemRepo.AddAsync(new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = ci.ProductId,
                            Quantity = ci.Quantity,
                            Price = ci.Product.Price
                        });
                    }
                    await _orderItemRepo.CommitAsync();

                    // Payment
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

                    // Clear cart
                    await _cartItemRepo.DeleteRangeAsync(cartItems.ToList());
                    await _cartItemRepo.CommitAsync();

                    // Add reward points
                    await AddPointsToUserAsync(userId, order.TotalAmount);

                    await trx.CommitAsync();

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
                    return StatusCode(500, new { msg = "Checkout failed", error = ex.Message });
                }
            }
        }
        #endregion
    }
}
