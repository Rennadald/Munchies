using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels.CustomMeal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Lunchbox.Controllers
{
    [Authorize(Roles = "parent")]
    public class CustomMealController : Controller
    {
        private readonly LunchboxContext _context;
        private const string CustomMealSessionKey = "custom_meal";

        public CustomMealController(LunchboxContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var items = await _context.Items
                .Include(i => i.Allergies)
                .ToListAsync();

            var categories = items.Select(i => i.ItemCategory).Distinct().ToList();

            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
            var children = await _context.Children
                .Where(c => c.ParentId == parent.ParentId)
                .Include(c => c.Allergies)
                .ToListAsync();

            ViewBag.Items = items;
            ViewBag.Categories = categories;
            ViewBag.Children = children;
            ViewBag.CustomMeal = GetCustomMeal();

            //return View();
            return View("~/Views/Order/Create.cshtml");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(AddItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data provided.";
                return RedirectToAction("Create");
            }

            try
            {
                var item = await _context.Items
                    .Include(i => i.Allergies)
                    .FirstOrDefaultAsync(i => i.ItemId == model.ItemId);

                if (item == null)
                {
                    TempData["Error"] = "Item not found.";
                    return RedirectToAction("Create");
                }

                var customMeal = GetCustomMeal();
                var itemKey = $"item_{model.ItemId}";

                if (customMeal.ContainsKey(itemKey))
                {
                    customMeal[itemKey].Quantity += model.Quantity;
                }
                else
                {
                    customMeal[itemKey] = new CustomMealItem
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        Price = item.UnitPrice ?? 0,
                        Quantity = model.Quantity,
                        Image = null, // Add image logic if needed
                        Calories = item.Calories ?? 0,
                        ProteinG = item.ProteinG ?? 0,
                        CarbsG = item.CarbsG ?? 0,
                        FatG = item.FatG ?? 0,
                        Allergies = item.Allergies.Select(a => a.AllergyType).ToList()
                    };
                }

                SaveCustomMeal(customMeal);
                TempData["Success"] = "Item added to your custom meal!";
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error adding item to custom meal.";
                return RedirectToAction("Create");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItem(string itemKey, string action)
        {
            var customMeal = GetCustomMeal();

            if (customMeal.ContainsKey(itemKey))
            {
                if (action == "increase")
                {
                    customMeal[itemKey].Quantity += 1;
                }
                else if (action == "decrease" && customMeal[itemKey].Quantity > 1)
                {
                    customMeal[itemKey].Quantity -= 1;
                }

                SaveCustomMeal(customMeal);
                TempData["Success"] = "Item quantity updated!";
            }

            return RedirectToAction("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(string itemKey)
        {
            var customMeal = GetCustomMeal();

            if (customMeal.ContainsKey(itemKey))
            {
                customMeal.Remove(itemKey);
                SaveCustomMeal(customMeal);
                TempData["Success"] = "Item removed from custom meal!";
            }

            return RedirectToAction("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CustomMealSessionKey);
            TempData["Success"] = "Custom meal cleared!";
            return RedirectToAction("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart()
        {
            var customMeal = GetCustomMeal();

            if (!customMeal.Any())
            {
                TempData["Error"] = "Your custom meal is empty!";
                return RedirectToAction("Create");
            }

            try
            {
                // Get current cart
                var cartJson = HttpContext.Session.GetString("cart");
                var cart = string.IsNullOrEmpty(cartJson)
                    ? new Dictionary<string, CartItem>()
                    : JsonSerializer.Deserialize<Dictionary<string, CartItem>>(cartJson);

                // Add each item from custom meal to cart
                foreach (var item in customMeal.Values)
                {
                    var cartItemKey = $"item_{item.ItemId}";

                    if (cart.ContainsKey(cartItemKey))
                    {
                        cart[cartItemKey].Quantity += item.Quantity;
                    }
                    else
                    {
                        cart[cartItemKey] = new CartItem
                        {
                            Type = "item",
                            ItemId = item.ItemId,
                            Name = item.Name,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            Image = item.Image,
                            Description = "Individual item from custom meal"
                        };
                    }
                }

                // Save cart and clear custom meal
                HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));
                HttpContext.Session.Remove(CustomMealSessionKey);

                TempData["Success"] = "All items from your custom meal have been added to cart!";
                return RedirectToAction("View", "Cart");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error adding custom meal to cart.";
                return RedirectToAction("Create");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFavorite(SaveFavoriteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data provided.";
                return RedirectToAction("Create");
            }

            var customMeal = GetCustomMeal();

            if (!customMeal.Any())
            {
                TempData["Error"] = "Your custom meal is empty!";
                return RedirectToAction("Create");
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
                    return RedirectToAction("Create");
                }

                // Create saved meal
                var savedMeal = new SavedMeal
                {
                    ParentId = parent.ParentId,
                    ChildId = child.ChildId,
                    Name = model.FavoriteName
                };

                _context.SavedMeals.Add(savedMeal);
                await _context.SaveChangesAsync();

                // Attach items to saved meal
                foreach (var item in customMeal.Values)
                {
                    var itemEntity = await _context.Items.FindAsync(item.ItemId);
                    if (itemEntity != null)
                    {
                        savedMeal.Items.Add(itemEntity);
                    }
                }

                await _context.SaveChangesAsync();

                // Clear custom meal
                HttpContext.Session.Remove(CustomMealSessionKey);

                TempData["Success"] = "Custom meal saved as favorite!";
                return RedirectToAction("Index", "SavedMeal");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving custom meal as favorite.";
                return RedirectToAction("Create");
            }
        }

        // Helper methods
        private Dictionary<string, CustomMealItem> GetCustomMeal()
        {
            var customMealJson = HttpContext.Session.GetString(CustomMealSessionKey);
            if (string.IsNullOrEmpty(customMealJson))
            {
                return new Dictionary<string, CustomMealItem>();
            }
            return JsonSerializer.Deserialize<Dictionary<string, CustomMealItem>>(customMealJson);
        }

        private void SaveCustomMeal(Dictionary<string, CustomMealItem> customMeal)
        {
            var customMealJson = JsonSerializer.Serialize(customMeal);
            HttpContext.Session.SetString(CustomMealSessionKey, customMealJson);
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }

    // Custom meal item model for session
    public class CustomMealItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
        public int Calories { get; set; }
        public decimal ProteinG { get; set; }
        public decimal CarbsG { get; set; }
        public decimal FatG { get; set; }
        public List<string> Allergies { get; set; }
    }
}