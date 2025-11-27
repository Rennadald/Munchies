using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels.Child;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Lunchbox.Controllers
{
    [Authorize(Roles = "parent")]
    public class ChildController : Controller
    {
        private readonly LunchboxContext _context;

        public ChildController(LunchboxContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            if (parent == null)
            {
                TempData["Error"] = "Parent profile not found.";
                return RedirectToAction("Dashboard", "Parent");
            }

            var children = await _context.Children
                .Where(c => c.ParentId == parent.ParentId)
                .Include(c => c.Allergies)
                .ToListAsync();

            var allergies = await _context.Allergies.ToListAsync();

            ViewBag.Allergies = allergies;
            return View(children);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateChildViewModel model)
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

                if (parent == null)
                {
                    TempData["Error"] = "Parent profile not found.";
                    return RedirectToAction("Index");
                }

                var child = new Child
                {
                    ParentId = parent.ParentId,
                    Name = model.Name,
                    DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth)
                };

                _context.Children.Add(child);
                await _context.SaveChangesAsync();

                // Attach allergies
                if (model.AllergyIds?.Any() == true)
                {
                    var allergies = await _context.Allergies
                        .Where(a => model.AllergyIds.Contains(a.AllergyId))
                        .ToListAsync();
                    child.Allergies = allergies;
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Child added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding child: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChild(int id)
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var child = await _context.Children
                .Include(c => c.Allergies)
                .FirstOrDefaultAsync(c => c.ChildId == id && c.ParentId == parent.ParentId);

            if (child == null)
            {
                return NotFound();
            }

            return Json(new
            {
                childId = child.ChildId,
                name = child.Name,
                dateOfBirth = child.DateOfBirth.ToString("yyyy-MM-dd"),
                allergyIds = child.Allergies.Select(a => a.AllergyId).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateChildViewModel model)
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

                var child = await _context.Children
                    .Include(c => c.Allergies)
                    .FirstOrDefaultAsync(c => c.ChildId == model.ChildId && c.ParentId == parent.ParentId);

                if (child == null)
                {
                    TempData["Error"] = "Unauthorized action.";
                    return RedirectToAction("Index");
                }

                child.Name = model.Name;
                child.DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth);

                // Update allergies
                child.Allergies.Clear();
                if (model.AllergyIds?.Any() == true)
                {
                    var allergies = await _context.Allergies
                        .Where(a => model.AllergyIds.Contains(a.AllergyId))
                        .ToListAsync();
                    child.Allergies = allergies;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Child updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating child: {ex.Message}";
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

                var child = await _context.Children
                    .Include(c => c.Allergies)
                    .FirstOrDefaultAsync(c => c.ChildId == id && c.ParentId == parent.ParentId);

                if (child == null)
                {
                    TempData["Error"] = "Unauthorized action.";
                    return RedirectToAction("Index");
                }

                child.Allergies.Clear();
                _context.Children.Remove(child);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Child deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting child: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}