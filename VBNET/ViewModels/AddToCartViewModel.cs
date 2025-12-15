using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Cart
{
    public class AddToCartViewModel
    {
        [Required(ErrorMessage = "Meal ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid meal ID")]
        public int MealId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
}
