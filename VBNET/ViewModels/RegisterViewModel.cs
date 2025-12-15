using Lunchbox.Validation;
using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(255, ErrorMessage = "First name cannot exceed 255 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(255, ErrorMessage = "Last name cannot exceed 255 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [PhoneNumber]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(255, ErrorMessage = "City cannot exceed 255 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "Street address is required")]
        [StringLength(500, ErrorMessage = "Street cannot exceed 500 characters")]
        public string Street { get; set; }

        [StringLength(500, ErrorMessage = "Apartment cannot exceed 500 characters")]
        public string Apartment { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
