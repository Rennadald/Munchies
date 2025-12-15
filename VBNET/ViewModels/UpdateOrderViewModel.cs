using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Admin.Order
{
    public class UpdateOrderViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Delivery date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Delivery Date")]
        public DateTime DeliveryDate { get; set; }

        [Required(ErrorMessage = "Delivery status is required")]
        [RegularExpression("^(Pending|Processing|Delivered|Cancelled)$",
            ErrorMessage = "Invalid delivery status")]
        [Display(Name = "Delivery Status")]
        public string DeliveryStatus { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive number")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
    }
}
