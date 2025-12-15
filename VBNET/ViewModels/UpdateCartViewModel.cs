using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Cart
{
    public class UpdateCartViewModel
    {
        [Required(ErrorMessage = "Cart key is required")]
        public string CartKey { get; set; }

        [Required(ErrorMessage = "Action is required")]
        [RegularExpression("^(increase|decrease)$", ErrorMessage = "Action must be 'increase' or 'decrease'")]
        public string Action { get; set; }
    }
}
