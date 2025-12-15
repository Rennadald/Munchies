using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Cart
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Payment method is required")]
        [RegularExpression("^(card|cash)$", ErrorMessage = "Payment method must be 'card' or 'cash'")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Delivery date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Delivery Date")]
        public DateTime DeliveryDate { get; set; }

        [Required(ErrorMessage = "Delivery time is required")]
        [Display(Name = "Delivery Time")]
        public string DeliveryTime { get; set; }
    }
}
