using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.SavedMeal
{
    public class SaveMealViewModel
    {
        [Required(ErrorMessage = "Meal name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please select a child")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid child ID")]
        public int ChildId { get; set; }

        [Required(ErrorMessage = "Please select at least one item")]
        [MinLength(1, ErrorMessage = "Please select at least one item")]
        public List<int> ItemIds { get; set; } = new List<int>();
    }
}
