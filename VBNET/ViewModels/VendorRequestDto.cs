using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lunchbox.Validation;

namespace Lunchbox.ViewModels
{
    public class VendorRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [PhoneNumber]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string Phone { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Admin|Vendor|Customer)$", ErrorMessage = "Role must be Admin, Vendor, or Customer")]
        public string Role { get; set; }

        // Vendor-specific fields
        [StringLength(255, ErrorMessage = "Pharmacy name cannot exceed 255 characters")]
        [Display(Name = "Pharmacy Name")]
        public string PharmacyName { get; set; }

        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string Location { get; set; }

        // Custom validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // If role is Vendor, require vendor-specific fields
            if (Role == "Vendor")
            {
                if (string.IsNullOrWhiteSpace(PharmacyName))
                {
                    yield return new ValidationResult(
                        "Pharmacy name is required for vendors",
                        new[] { nameof(PharmacyName) }
                    );
                }

                if (string.IsNullOrWhiteSpace(LicenseNumber))
                {
                    yield return new ValidationResult(
                        "License number is required for vendors",
                        new[] { nameof(LicenseNumber) }
                    );
                }

                if (string.IsNullOrWhiteSpace(Location))
                {
                    yield return new ValidationResult(
                        "Location is required for vendors",
                        new[] { nameof(Location) }
                    );
                }
            }
        }
    }
}