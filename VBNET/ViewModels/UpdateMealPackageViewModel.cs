using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Admin.MealPackage
{
    public class UpdateMealPackageViewModel
    {
        [Required]
        public int MealPackageId { get; set; }

        [Required(ErrorMessage = "Package name is required")]
        [StringLength(255, ErrorMessage = "Package name cannot exceed 255 characters")]
        [Display(Name = "Package Name")]
        public string PackageName { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Package Description")]
        public string PackageDescription { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Meals per day is required")]
        [Range(1, 10, ErrorMessage = "Meals per day must be between 1 and 10")]
        [Display(Name = "Meals Per Day")]
        public int MealsPerDay { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        [Display(Name = "Discount Percent")]
        public decimal? DiscountPercent { get; set; }
    }
}
