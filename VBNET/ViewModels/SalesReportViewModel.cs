using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Admin.Report
{
    public class SalesReportViewModel
    {
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Report type is required")]
        [RegularExpression("^(summary|detailed|export)$",
            ErrorMessage = "Report type must be summary, detailed, or export")]
        [Display(Name = "Report Type")]
        public string ReportType { get; set; }
    }

    // Custom validation attribute to ensure EndDate is after or equal to StartDate
    public class DateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (SalesReportViewModel)validationContext.ObjectInstance;

            if (model.EndDate < model.StartDate)
            {
                return new ValidationResult("End date must be after or equal to start date");
            }

            return ValidationResult.Success;
        }
    }
}
