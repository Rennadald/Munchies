using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Lunchbox.Controllers
{
    [Authorize(Roles = "parent")]
    public class ReviewController : Controller
    {
        private readonly LunchboxContext _context;

        public ReviewController(LunchboxContext context)
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

            // Get all reviews by this parent
            var reviews = await _context.Ratings
                .Include(r => r.Order)
                    .ThenInclude(o => o.Child)
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.PreMadeMeal)
                .Where(r => r.Order.ParentId == parent.ParentId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

            var review = await _context.Ratings
                .Include(r => r.Order)
                    .ThenInclude(o => o.Child)
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                .Include(r => r.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.PreMadeMeal)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (review == null)
            {
                return NotFound();
            }

            // Verify review belongs to parent
            if (review.Order.ParentId != parent.ParentId)
            {
                TempData["Error"] = "Unauthorized action.";
                return RedirectToAction("Index");
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Edit", new { id = model.RatingId });
            }

            try
            {
                var userId = GetCurrentUserId();
                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);

                var review = await _context.Ratings
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.RatingId == model.RatingId);

                if (review == null)
                {
                    TempData["Error"] = "Review not found.";
                    return RedirectToAction("Index");
                }

                // Verify review belongs to parent
                if (review.Order.ParentId != parent.ParentId)
                {
                    TempData["Error"] = "Unauthorized action.";
                    return RedirectToAction("Index");
                }

                review.RatingStars = model.Rating;
                review.Comment = model.Comment;
                review.RatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Review updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating review: {ex.Message}";
                return RedirectToAction("Edit", new { id = model.RatingId });
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

                var review = await _context.Ratings
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.RatingId == id);

                if (review == null)
                {
                    TempData["Error"] = "Review not found.";
                    return RedirectToAction("Index");
                }

                // Verify review belongs to parent
                if (review.Order.ParentId != parent.ParentId)
                {
                    TempData["Error"] = "Unauthorized action.";
                    return RedirectToAction("Index");
                }

                _context.Ratings.Remove(review);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Review deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting review: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}