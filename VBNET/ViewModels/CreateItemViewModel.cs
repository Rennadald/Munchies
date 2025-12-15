using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Admin.Item
{
    public class CreateItemViewModel
    {
        [Required(ErrorMessage = "Item name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(255, ErrorMessage = "Category cannot exceed 255 characters")]
        [Display(Name = "Item Category")]
        public string ItemCategory { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Calories is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Calories must be a positive number")]
        public int Calories { get; set; }

        [Required(ErrorMessage = "Protein is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Protein must be a positive number")]
        [Display(Name = "Protein (g)")]
        public decimal ProteinG { get; set; }

        [Required(ErrorMessage = "Fat is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Fat must be a positive number")]
        [Display(Name = "Fat (g)")]
        public decimal FatG { get; set; }

        [Required(ErrorMessage = "Carbs is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Carbs must be a positive number")]
        [Display(Name = "Carbs (g)")]
        public decimal CarbsG { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Sugar must be a positive number")]
        [Display(Name = "Sugar (g)")]
        public decimal? SugarG { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Sodium must be a positive number")]
        [Display(Name = "Sodium (mg)")]
        public decimal? SodiumMg { get; set; }

        [Display(Name = "Allergies")]
        public List<int> AllergyIds { get; set; } = new List<int>();
    }
}
