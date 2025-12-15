using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Cart
{
    public class SelectChildViewModel
    {
        [Required(ErrorMessage = "Please select a child")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid child ID")]
        public int ChildId { get; set; }
    }
}
