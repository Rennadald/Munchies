using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Child
{
    public class CreateChildViewModel
    {
        [Required(ErrorMessage = "Child name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Allergies")]
        public List<int> AllergyIds { get; set; } = new List<int>();
    }
}
