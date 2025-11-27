using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Lunchbox.Controllers
{
    [Authorize(Roles = "parent")]
    public class CartController : Controller
    {
        private readonly LunchboxContext _context;
        private const string CartSessionKey = "cart";
        private const string SelectedChildKey = "selected_child_id";

        public CartController(LunchboxContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> View()
        {
            var cart = GetCart();
            decimal total = 0;

            foreach (var item in cart.Values)
            {
                total += item.Price * item.Quantity;
            }

            // Get parent's children
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
            var children = await _context.Children
                .Where(c => c.ParentId == parent.ParentId)
                .Include(c => c.Allergies)
                .ToListAsync();

            // Get selected child
            Child selectedChild = null;
            var selectedChildId = HttpContext.Session.GetInt32(SelectedChildKey);
            if (selectedChildId.HasValue)
            {
                selectedChild = await _context.Children.FindAsync(selectedChildId.Value);
            }

            ViewBag.Cart = cart;
            ViewBag.Total = total;
            ViewBag.Children = children;
            ViewBag.SelectedChild = selectedChild;

            return await View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddToCartViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data provided.";
                return RedirectToAction("Lunchboxes", "Home");
            }

            try
            {
                var meal = await _context.PreMadeMeals.FindAsync(model.MealId);
                if (meal == null)
                {
                    TempData["Error"] = "Meal not found.";
                    return RedirectToAction("Lunchboxes", "Home");
                }

                var cart = GetCart();
                var cartKey = $"meal_{model.MealId}";

                if (cart.ContainsKey(cartKey))
                {
                    cart[cartKey].Quantity += model.Quantity;
                }
                else
                {
                    cart[cartKey] = new CartItem
                    {
                        Type = "premade_meal",
                        MealId = meal.PreMadeMealId,
                        Name = meal.MealName,
                        Price = meal.FixedPrice ?? 0,
                        Quantity = model.Quantity,
                        Image = meal.ImageUrl,
                        Description = meal.MealDescription
                    };
                }

                SaveCart(cart);
                TempData["Success"] = "Meal added to cart successfully!";
                return RedirectToAction("View");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error adding meal to cart.";
                return RedirectToAction("Lunchboxes", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(UpdateCartViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid action.";
                return RedirectToAction("View");
            }

            var cart = GetCart();

            if (cart.ContainsKey(model.CartKey))
            {
                if (model.Action == "increase")
                {
                    cart[model.CartKey].Quantity += 1;
                }
                else if (model.Action == "decrease" && cart[model.CartKey].Quantity > 1)
                {
                    cart[model.CartKey].Quantity -= 1;
                }

                SaveCart(cart);
                TempData["Success"] = "Cart updated successfully!";
            }

            return RedirectToAction("View");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string cartKey)
        {
            var cart = GetCart();

            if (cart.ContainsKey(cartKey))
            {
                cart.Remove(cartKey);
                SaveCart(cart);
                TempData["Success"] = "Item removed from cart!";
            }

            return RedirectToAction("View");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(SelectedChildKey);
            TempData["Success"] = "Cart cleared successfully!";
            return RedirectToAction("View");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateChild(SelectChildViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please select a valid child.";
                return RedirectToAction("View");
            }

            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
            var child = await _context.Children
                .FirstOrDefaultAsync(c => c.ChildId == model.ChildId && c.ParentId == parent.ParentId);

            if (child == null)
            {
                TempData["Error"] = "Child not found.";
                return RedirectToAction("View");
            }

            HttpContext.Session.SetInt32(SelectedChildKey, child.ChildId);
            TempData["Success"] = "Child selected for order!";
            return RedirectToAction("View");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            var selectedChildId = HttpContext.Session.GetInt32(SelectedChildKey);

            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("View");
            }

            if (!selectedChildId.HasValue)
            {
                TempData["Error"] = "Please select a child for this order.";
                return RedirectToAction("View");
            }

            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
            var child = await _context.Children
                .Include(c => c.Allergies)  // Add this
                .FirstOrDefaultAsync(c => c.ChildId == selectedChildId.Value && c.ParentId == parent.ParentId);

            if (child == null)
            {
                TempData["Error"] = "Invalid child selected.";
                return RedirectToAction("View");
            }

            decimal total = cart.Values.Sum(item => item.Price * item.Quantity);

            ViewBag.Cart = cart;
            ViewBag.Total = total;
            ViewBag.Child = child;

            return View(new CheckoutViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Checkout");
            }

            // Additional validation
            if (model.DeliveryDate <= DateTime.Today)
            {
                TempData["Error"] = "Delivery date must be in the future.";
                return RedirectToAction("Checkout");
            }

            var cart = GetCart();
            var selectedChildId = HttpContext.Session.GetInt32(SelectedChildKey);

            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("View");
            }

            if (!selectedChildId.HasValue)
            {
                TempData["Error"] = "Please select a child for this order.";
                return RedirectToAction("View");
            }

            try
            {
                var userId = GetCurrentUserId();
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
                decimal total = cart.Values.Sum(item => item.Price * item.Quantity);

                // Create order
                var order = new Order
                {
                    ParentId = parent.ParentId,
                    ChildId = selectedChildId.Value,
                    OrderDate = DateTime.Now,
                    DeliveryDate = model.DeliveryDate,
                    DeliveryStatus = "Pending",
                    TotalAmount = total
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items
                foreach (var item in cart.Values)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        PreMadeMealId = item.Type == "premade_meal" ? item.MealId : null,
                        ItemId = item.Type == "item" ? item.ItemId : null,
                        Quantity = item.Quantity
                    };
                    _context.OrderItems.Add(orderItem);
                }

                // Create payment record
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = total,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = model.PaymentMethod,
                    Status = model.PaymentMethod == "cash" ? "Pending" : "Completed"
                };
                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();

                // Clear cart and session
                HttpContext.Session.Remove(CartSessionKey);
                HttpContext.Session.Remove(SelectedChildKey);

                TempData["Success"] = "Order placed successfully!";
                return RedirectToAction("Confirmation", "Order", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error processing order. Please try again.";
                return RedirectToAction("Checkout");
            }
        }

        // Helper methods
        private Dictionary<string, CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new Dictionary<string, CartItem>();
            }
            return JsonSerializer.Deserialize<Dictionary<string, CartItem>>(cartJson);
        }

        private void SaveCart(Dictionary<string, CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }

    // Cart item model for session
    public class CartItem
    {
        public string Type { get; set; }
        public int? MealId { get; set; }
        public int? ItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}