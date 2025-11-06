using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

public class RegisterViewModel : IValidatableObject
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters and include a lowercase letter and a digit.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "South African ID number")]
    public string? SaIdNumber { get; set; }

    [Display(Name = "Passport number (if no SA ID)")]
    public string? PassportNumber { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Residential address")]
    [DataType(DataType.MultilineText)]
    public string ResidentialAddress { get; set; } = string.Empty;

    [Display(Name = "I consent to provide Employment Equity information")]
    public bool EquityConsent { get; set; }

    [Display(Name = "Ethnicity (optional)")]
    public string? EquityEthnicity { get; set; }

    [Display(Name = "Gender (optional)")]
    public string? EquityGender { get; set; }

    [Display(Name = "Disability Status (optional)")]
    public string? EquityDisability { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(SaIdNumber) && string.IsNullOrWhiteSpace(PassportNumber))
        {
            yield return new ValidationResult(
                "Please provide either a South African ID number or a passport number.",
                new[] { nameof(SaIdNumber), nameof(PassportNumber) });
        }

        if (!string.IsNullOrWhiteSpace(SaIdNumber) && SaIdNumber!.Length != 13)
        {
            yield return new ValidationResult(
                "South African ID numbers must be 13 digits long.",
                new[] { nameof(SaIdNumber) });
        }
    }
}
