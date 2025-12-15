using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Admin.Order
{
    public class UpdateOrderStatusViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Delivery status is required")]
        [RegularExpression("^(Pending|Processing|Delivered|Cancelled)$",
            ErrorMessage = "Status must be Pending, Processing, Delivered, or Cancelled")]
        [Display(Name = "Delivery Status")]
        public string DeliveryStatus { get; set; }
    }
}
