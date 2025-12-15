using Lunchbox.Models;

namespace Lunchbox.ViewModels
{
    public class MealWithRatingViewModel
    {
        public PreMadeMeal Meal { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
