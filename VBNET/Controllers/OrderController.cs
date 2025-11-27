using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Lunchbox.Controllers
{
    [Authorize(Roles = "parent")]
    public class OrderController : Controller
    {
        private readonly LunchboxContext _context;

        public OrderController(LunchboxContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var order = await _context.Orders
                .Include(o => o.Parent)
                .Include(o => o.Child)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.PreMadeMeal)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Verify order belongs to current parent
            if (order.ParentId != parent.ParentId)
            {
                return Forbid();
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string status)
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var query = _context.Orders
                .Where(o => o.ParentId == parent.ParentId)
                .Include(o => o.Child)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.PreMadeMeal)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.OrderDate);

            if (!string.IsNullOrEmpty(status))
            {
                query = (IOrderedQueryable<Order>)query.Where(o => o.DeliveryStatus == status);
            }

            var orders = await query.ToListAsync();
            ViewBag.CurrentStatus = status;

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Show(int id)
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var order = await _context.Orders
                .Include(o => o.Parent)
                .Include(o => o.Child)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.PreMadeMeal)
                .Include(o => o.Payments)
                .Include(o => o.Ratings)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.ParentId != parent.ParentId)
            {
                return Forbid();
            }

            return View("Confirmation", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.PreMadeMeal)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("Index");
                }

                if (order.ParentId != parent.ParentId)
                {
                    TempData["Error"] = "Unauthorized access to this order.";
                    return RedirectToAction("Index");
                }

                // Get cart from session
                var cartJson = HttpContext.Session.GetString("cart");
                var cart = string.IsNullOrEmpty(cartJson)
                    ? new Dictionary<string, CartItem>()
                    : JsonSerializer.Deserialize<Dictionary<string, CartItem>>(cartJson);

                // Add order items to cart
                foreach (var item in order.OrderItems)
                {
                    if (item.PreMadeMeal != null)
                    {
                        var cartKey = $"meal_{item.PreMadeMeal.PreMadeMealId}";
                        if (cart.ContainsKey(cartKey))
                        {
                            cart[cartKey].Quantity += item.Quantity ?? 1; // FIXED
                        }
                        else
                        {
                            cart[cartKey] = new CartItem
                            {
                                Type = "premade_meal",
                                MealId = item.PreMadeMeal.PreMadeMealId,
                                Name = item.PreMadeMeal.MealName,
                                Price = item.PreMadeMeal.FixedPrice ?? 0,
                                Quantity = item.Quantity ?? 1, // FIXED
                                Image = item.PreMadeMeal.ImageUrl,
                                Description = item.PreMadeMeal.MealDescription
                            };
                        }
                    }
                    else if (item.Item != null)
                    {
                        var cartKey = $"item_{item.Item.ItemId}";
                        if (cart.ContainsKey(cartKey))
                        {
                            cart[cartKey].Quantity += item.Quantity ?? 1; // FIXED
                        }
                        else
                        {
                            cart[cartKey] = new CartItem
                            {
                                Type = "item",
                                ItemId = item.Item?.ItemId ?? 0,
                                Name = item.Item?.Name ?? "",
                                Price = item.Item?.UnitPrice ?? 0,
                                Quantity = item.Quantity ?? 1, // FIXED
                                Description = item.Item?.Description
                            };
                        }
                    }
                }

                // Save cart
                HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));

                TempData["Success"] = "Order items added to cart successfully!";
                return RedirectToAction("View", "Cart");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error reordering items.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(RateOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid rating data.";
                return RedirectToAction("Show", new { id = model.OrderId });
            }

            try
            {
                var userId = GetCurrentUserId();
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

                var order = await _context.Orders
                    .Include(o => o.Ratings)
                    .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("Index");
                }

                if (order.ParentId != parent.ParentId)
                {
                    TempData["Error"] = "Unauthorized access to this order.";
                    return RedirectToAction("Index");
                }

                // Create rating
                var rating = new Rating
                {
                    OrderId = order.OrderId,
                    RatingStars = model.Rating,
                    Comment = model.Comment,
                    RatedAt = DateTime.Now
                };

                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thank you for your rating!";
                return RedirectToAction("Show", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error submitting rating.";
                return RedirectToAction("Show", new { id = model.OrderId });
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}