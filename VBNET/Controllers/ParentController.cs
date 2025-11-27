using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels.Parent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BC = BCrypt.Net.BCrypt;

namespace Lunchbox.Controllers
{
    [Authorize(Roles = "parent")]
    public class ParentController : Controller
    {
        private readonly LunchboxContext _context;

        public ParentController(LunchboxContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (parent == null)
            {
                TempData["Error"] = "Parent profile not found.";
                return RedirectToAction("Login", "Auth");
            }

            // Get recent orders
            var recentOrders = await _context.Orders
                .Where(o => o.ParentId == parent.ParentId)
                .Include(o => o.Child)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.PreMadeMeal)
                .OrderByDescending(o => o.DeliveryDate)
                .Take(5)
                .ToListAsync();

            // Get statistics
            var childrenCount = parent.Children.Count;
            var totalOrders = await _context.Orders.CountAsync(o => o.ParentId == parent.ParentId);
            var upcomingDeliveries = await _context.Orders
                .Where(o => o.ParentId == parent.ParentId &&
                           o.DeliveryDate.HasValue &&
                           o.DeliveryDate.Value >= DateTime.Today &&
                           o.DeliveryDate.Value <= DateTime.Today.AddDays(7) &&
                           (o.DeliveryStatus == "Pending" || o.DeliveryStatus == "Processing"))
                .CountAsync();

            ViewBag.Parent = parent;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.ChildrenCount = childrenCount;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.UpcomingDeliveries = upcomingDeliveries;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MealTracking(int week = 0)
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            if (parent == null)
            {
                TempData["Error"] = "Parent profile not found.";
                return RedirectToAction("Dashboard");
            }

            // Calculate week dates
            var startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).AddDays(week * 7);
            var endDate = startDate.AddDays(6);

            // Get children with their orders for the week
            var children = await _context.Children
                .Where(c => c.ParentId == parent.ParentId)
                .Include(c => c.Orders.Where(o =>
                    o.DeliveryDate.HasValue &&
                    o.DeliveryDate.Value >= startDate &&
                    o.DeliveryDate.Value <= endDate))
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                .Include(c => c.Orders.Where(o =>
                    o.DeliveryDate.HasValue &&
                    o.DeliveryDate.Value >= startDate &&
                    o.DeliveryDate.Value <= endDate))
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.PreMadeMeal)
                .ToListAsync();

            // Generate week days
            var weekDays = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                weekDays.Add(startDate.AddDays(i));
            }

            ViewBag.Children = children;
            ViewBag.WeekDays = weekDays;
            ViewBag.WeekOffset = week;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChildMealTracking(int id, int week = 0)
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var child = await _context.Children
                .FirstOrDefaultAsync(c => c.ChildId == id && c.ParentId == parent.ParentId);

            if (child == null)
            {
                TempData["Error"] = "Unauthorized action.";
                return RedirectToAction("MealTracking");
            }

            // Calculate week dates
            var startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).AddDays(week * 7);
            var endDate = startDate.AddDays(6);

            // Get orders for the child in the date range
            var orders = await _context.Orders
                .Where(o => o.ChildId == child.ChildId &&
                           o.DeliveryDate.HasValue &&
                           o.DeliveryDate.Value >= startDate &&
                           o.DeliveryDate.Value <= endDate)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.PreMadeMeal)
                .Include(o => o.Package)
                .OrderBy(o => o.DeliveryDate)
                .ToListAsync();

            // Group orders by date
            var mealsByDate = new Dictionary<string, List<Order>>();
            foreach (var order in orders)
            {
                var date = order.DeliveryDate.Value.ToString("yyyy-MM-dd");
                if (!mealsByDate.ContainsKey(date))
                {
                    mealsByDate[date] = new List<Order>();
                }
                mealsByDate[date].Add(order);
            }

            // Generate week days
            var weekDays = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                weekDays.Add(startDate.AddDays(i));
            }

            ViewBag.Child = child;
            ViewBag.MealsByDate = mealsByDate;
            ViewBag.WeekDays = weekDays;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.WeekOffset = week;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            if (parent == null)
            {
                TempData["Error"] = "Parent profile not found.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.User = user;
            ViewBag.Parent = parent;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Profile");
            }

            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

                if (user == null || parent == null)
                {
                    TempData["Error"] = "Profile not found.";
                    return RedirectToAction("Profile");
                }

                // Check if email is unique
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == model.Email && u.UserId != userId);

                if (emailExists)
                {
                    TempData["Error"] = "Email already in use.";
                    return RedirectToAction("Profile");
                }

                // Update User
                user.FullName = $"{model.FirstName} {model.LastName}";
                user.Email = model.Email;
                user.UpdatedAt = DateTime.Now;

                // Update Parent
                parent.FirstName = model.FirstName;
                parent.LastName = model.LastName;
                parent.Email = model.Email;
                parent.PhoneNumber = model.PhoneNumber;
                parent.DeliveryAddress = model.DeliveryAddress;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating profile: {ex.Message}";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Profile");
            }

            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Profile");
                }

                // Verify current password
                if (!BC.Verify(model.CurrentPassword, user.Password))
                {
                    TempData["Error"] = "Current password is incorrect.";
                    return RedirectToAction("Profile");
                }

                // Update password
                user.Password = BC.HashPassword(model.Password);
                user.UpdatedAt = DateTime.Now;

                // Also update parent password if exists
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
                if (parent != null)
                {
                    parent.Password = BC.HashPassword(model.Password);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Password updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating password: {ex.Message}";
                return RedirectToAction("Profile");
            }
        }

        [HttpGet]
        public IActionResult Children()
        {
            return RedirectToAction("Index", "Child");
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}