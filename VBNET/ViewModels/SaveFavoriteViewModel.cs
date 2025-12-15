using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.CustomMeal
{
    public class SaveFavoriteViewModel
    {
        [Required(ErrorMessage = "Favorite name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        [Display(Name = "Favorite Name")]
        public string FavoriteName { get; set; }

        [Required(ErrorMessage = "Please select a child")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid child ID")]
        public int ChildId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }
    }
}
