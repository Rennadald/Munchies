using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Order
{
    public class RateOrderViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }
    }
}
