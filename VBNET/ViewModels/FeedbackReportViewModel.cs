using System.ComponentModel.DataAnnotations;

namespace Lunchbox.ViewModels.Admin.Report
{
    public class FeedbackReportViewModel
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
}
