using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels.SavedMeal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Lunchbox.Controllers
{
    [Authorize(Roles = "parent")]
    public class SavedMealController : Controller
    {
        private readonly LunchboxContext _context;

        public SavedMealController(LunchboxContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var savedMeals = await _context.SavedMeals
                .Where(sm => sm.ParentId == parent.ParentId)
                .Include(sm => sm.Child)
                .Include(sm => sm.Items)
                .ToListAsync();

            var children = await _context.Children
                .Where(c => c.ParentId == parent.ParentId)
                .Include(c => c.Allergies)
                .ToListAsync();

            ViewBag.SavedMeals = savedMeals;
            ViewBag.Children = children;

            //return View();
            return View("~/Views/Favorites/Index.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SaveMealViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data provided.";
                return RedirectToAction("Index");
            }

            try
            {
                var userId = GetCurrentUserId();
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

                // Verify child belongs to parent
                var child = await _context.Children
                    .FirstOrDefaultAsync(c => c.ChildId == model.ChildId && c.ParentId == parent.ParentId);

                if (child == null)
                {
                    TempData["Error"] = "Invalid child selected.";
                    return RedirectToAction("Index");
                }

                // Create saved meal
                var savedMeal = new SavedMeal
                {
                    ParentId = parent.ParentId,
                    ChildId = child.ChildId,
                    Name = model.Name
                };

                _context.SavedMeals.Add(savedMeal);
                await _context.SaveChangesAsync();

                // Attach items
                var items = await _context.Items
                    .Where(i => model.ItemIds.Contains(i.ItemId))
                    .ToListAsync();
                savedMeal.Items = items;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Meal saved to favorites!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving meal to favorites.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

                var savedMeal = await _context.SavedMeals
                    .Include(sm => sm.Items)
                    .FirstOrDefaultAsync(sm => sm.SavedMealId == id && sm.ParentId == parent.ParentId);

                if (savedMeal == null)
                {
                    TempData["Error"] = "Saved meal not found.";
                    return RedirectToAction("Index");
                }

                _context.SavedMeals.Remove(savedMeal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Saved meal removed from favorites!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error removing saved meal from favorites.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

                var savedMeal = await _context.SavedMeals
                    .Include(sm => sm.Items)
                    .FirstOrDefaultAsync(sm => sm.SavedMealId == id && sm.ParentId == parent.ParentId);

                if (savedMeal == null)
                {
                    TempData["Error"] = "Saved meal not found.";
                    return RedirectToAction("Index");
                }

                // Get cart from session
                var cartJson = HttpContext.Session.GetString("cart");
                var cart = string.IsNullOrEmpty(cartJson)
                    ? new Dictionary<string, CartItem>()
                    : JsonSerializer.Deserialize<Dictionary<string, CartItem>>(cartJson);

                // Add items to cart
                foreach (var item in savedMeal.Items)
                {
                    var cartKey = $"item_{item.ItemId}";
                    if (cart.ContainsKey(cartKey))
                    {
                        cart[cartKey].Quantity += 1;
                    }
                    else
                    {
                        cart[cartKey] = new CartItem
                        {
                            Type = "item",
                            ItemId = item.ItemId,
                            Name = item.Name,
                            Price = item.UnitPrice ?? 0,
                            Quantity = 1,
                            Description = item.Description
                        };
                    }
                }

                // Save cart
                HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));

                TempData["Success"] = "Saved meal items added to cart!";
                return RedirectToAction("View", "Cart");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error reordering saved meal.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var children = await _context.Children
                .Where(c => c.ParentId == parent.ParentId)
                .Include(c => c.Allergies)
                .ToListAsync();

            var items = await _context.Items
                .Include(i => i.Allergies)
                .ToListAsync();

            var groupedItems = items.GroupBy(i => i.ItemCategory)
                .ToDictionary(g => g.Key, g => g.ToList());

            var customMealJson = HttpContext.Session.GetString("custom_meal");
            var customMealItems = string.IsNullOrEmpty(customMealJson)
                ? new Dictionary<string, CustomMealItem>()
                : JsonSerializer.Deserialize<Dictionary<string, CustomMealItem>>(customMealJson);

            ViewBag.Children = children;
            ViewBag.Items = groupedItems;
            ViewBag.CustomMealItems = customMealItems;

            //return View();
            return View("~/Views/Favorites/Create.cshtml");
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}