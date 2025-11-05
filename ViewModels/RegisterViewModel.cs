using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "South African ID Number")]
    public string SaIdNumber { get; set; } = string.Empty;

    [Display(Name = "I consent to provide Employment Equity information")]
    public bool EquityConsent { get; set; }

    [Display(Name = "Ethnicity (optional)")]
    public string? EquityEthnicity { get; set; }

    [Display(Name = "Gender (optional)")]
    public string? EquityGender { get; set; }

    [Display(Name = "Disability Status (optional)")]
    public string? EquityDisability { get; set; }
}
