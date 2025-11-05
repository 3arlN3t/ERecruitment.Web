using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

/// <summary>
/// ViewModel for creating government-compliant job postings.
/// Structured to match official government recruitment document format.
/// </summary>
public class CreateJobPostingViewModel
{
    // PRIMARY IDENTIFICATION
    [Required(ErrorMessage = "Post Number is required")]
    [Display(Name = "Post Number")]
    [StringLength(50, ErrorMessage = "Post Number cannot exceed 50 characters")]
    public string PostNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Reference Number is required")]
    [Display(Name = "Reference Number")]
    [StringLength(50, ErrorMessage = "Reference Number cannot exceed 50 characters")]
    public string ReferenceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job Title is required")]
    [Display(Name = "Job Title")]
    [StringLength(300, ErrorMessage = "Job Title cannot exceed 300 characters")]
    public string Title { get; set; } = string.Empty;

    // SECTION 1: SALARY
    [Required(ErrorMessage = "Salary Range is required")]
    [Display(Name = "Salary Range")]
    [DataType(DataType.MultilineText)]
    [StringLength(2000, ErrorMessage = "Salary Range cannot exceed 2000 characters")]
    public string SalaryRange { get; set; } = string.Empty;

    // SECTION 2: CENTRE
    [Required(ErrorMessage = "Work Centre/Location is required")]
    [Display(Name = "Work Centre/Location")]
    [StringLength(200, ErrorMessage = "Centre cannot exceed 200 characters")]
    public string Centre { get; set; } = string.Empty;

    [Display(Name = "Province")]
    [StringLength(50, ErrorMessage = "Province cannot exceed 50 characters")]
    public string? Province { get; set; }

    // SECTION 3: REQUIREMENTS
    [Required(ErrorMessage = "Requirements are required")]
    [Display(Name = "Requirements")]
    [DataType(DataType.MultilineText)]
    [StringLength(10000, ErrorMessage = "Requirements cannot exceed 10000 characters")]
    public string Requirements { get; set; } = string.Empty;

    // SECTION 4: DUTIES
    [Required(ErrorMessage = "Duties and Responsibilities are required")]
    [Display(Name = "Duties and Responsibilities")]
    [DataType(DataType.MultilineText)]
    [StringLength(10000, ErrorMessage = "Duties Description cannot exceed 10000 characters")]
    public string DutiesDescription { get; set; } = string.Empty;

    // SECTION 5: ENQUIRIES
    [Required(ErrorMessage = "Enquiries Contact Person is required")]
    [Display(Name = "Enquiries Contact Person")]
    [StringLength(200, ErrorMessage = "Contact Person name cannot exceed 200 characters")]
    public string EnquiriesContactPerson { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enquiries Phone Number is required")]
    [Display(Name = "Enquiries Phone Number")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string EnquiriesPhone { get; set; } = string.Empty;

    // SECTION 6: APPLICATIONS
    [Required(ErrorMessage = "Application Email is required")]
    [Display(Name = "Application Email Address")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string ApplicationEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Closing Date is required")]
    [Display(Name = "Closing Date")]
    [DataType(DataType.Date)]
    public DateTime ClosingDate { get; set; } = DateTime.UtcNow.AddDays(30);

    // SECTION 7: NOTES
    [Display(Name = "Additional Notes")]
    [DataType(DataType.MultilineText)]
    [StringLength(2000, ErrorMessage = "Additional Notes cannot exceed 2000 characters")]
    public string? AdditionalNotes { get; set; }

    // LEGACY FIELDS (for backward compatibility)
    [Display(Name = "Department/Ministry")]
    [StringLength(200, ErrorMessage = "Department cannot exceed 200 characters")]
    public string Department { get; set; } = string.Empty;

    [Display(Name = "Screening Questions (one per line)")]
    [DataType(DataType.MultilineText)]
    public string? KillerQuestionsText { get; set; }
}

