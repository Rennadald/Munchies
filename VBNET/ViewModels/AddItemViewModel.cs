using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.CustomMeal
{
    public class AddItemViewModel
    {
        [Required(ErrorMessage = "Item ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid item ID")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
}
