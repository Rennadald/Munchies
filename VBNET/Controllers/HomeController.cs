using Lunchbox.Data;
using Lunchbox.Models;
using Lunchbox.ViewModels;  // ADD THIS LINE
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lunchbox.Controllers
{
    public class HomeController : Controller
    {
        private readonly LunchboxContext _context;

        public HomeController(LunchboxContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var mealPackages = await _context.MealPackages.Take(3).ToListAsync();

            var topMeals = await _context.PreMadeMeals
                .Where(m => m.IsAvailable == true)
                .Include(m => m.Items)
                .Include(m => m.OrderItems)
                    .ThenInclude(oi => oi.Order)
                        .ThenInclude(o => o.Ratings)
                .Take(3)
                .ToListAsync();

            var topMealsWithRatings = topMeals.Select(m => new MealWithRatingViewModel
            {
                Meal = m,
                AverageRating = m.OrderItems
                    .Where(oi => oi.Order != null && oi.Order.Ratings != null && oi.Order.Ratings.Any())
                    .SelectMany(oi => oi.Order.Ratings)
                    .Select(r => (double)r.RatingStars)
                    .DefaultIfEmpty(4.0)
                    .Average(),
                RatingCount = m.OrderItems
                    .Where(oi => oi.Order != null && oi.Order.Ratings != null)
                    .SelectMany(oi => oi.Order.Ratings)
                    .Count()
            })
            .OrderByDescending(x => x.AverageRating)
            .ToList();

            ViewBag.MealPackages = mealPackages;
            ViewBag.TopMeals = topMealsWithRatings;

            return View();
        }

        public async Task<IActionResult> Lunchboxes()
        {
            // Load all meals with their related data
            var meals = await _context.PreMadeMeals
                .Where(m => m.IsAvailable == true)
                .Include(m => m.Items)
                .Include(m => m.OrderItems)
                    .ThenInclude(oi => oi.Order)
                        .ThenInclude(o => o.Ratings)
                .ToListAsync();

            // Calculate ratings in memory to avoid complex query issues
            var mealsWithRatings = meals.Select(m => new MealWithRatingViewModel
            {
                Meal = m,
                AverageRating = m.OrderItems
                    .Where(oi => oi.Order != null && oi.Order.Ratings != null && oi.Order.Ratings.Any())
                    .SelectMany(oi => oi.Order.Ratings)
                    .Select(r => (double)r.RatingStars)
                    .DefaultIfEmpty(0.0)
                    .Average(),
                RatingCount = m.OrderItems
                    .Where(oi => oi.Order != null && oi.Order.Ratings != null)
                    .SelectMany(oi => oi.Order.Ratings)
                    .Count()
            }).ToList();

            return View(mealsWithRatings);
        }

        public async Task<IActionResult> Packages()
        {
            var packages = await _context.MealPackages.ToListAsync();
            return View(packages);
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}