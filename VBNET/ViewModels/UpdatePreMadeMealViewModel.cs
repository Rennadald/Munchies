using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Admin.PreMadeMeal
{
    public class UpdatePreMadeMealViewModel
    {
        [Required]
        public int PreMadeMealId { get; set; }

        [Required(ErrorMessage = "Meal name is required")]
        [StringLength(255, ErrorMessage = "Meal name cannot exceed 255 characters")]
        [Display(Name = "Meal Name")]
        public string MealName { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Meal Description")]
        public string MealDescription { get; set; }

        [Required(ErrorMessage = "Fixed price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        [Display(Name = "Fixed Price")]
        public decimal FixedPrice { get; set; }

        [Required(ErrorMessage = "Availability status is required")]
        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Please select at least one item")]
        [MinLength(1, ErrorMessage = "Please select at least one item")]
        [Display(Name = "Items")]
        public List<int> ItemIds { get; set; } = new List<int>();
    }
}
