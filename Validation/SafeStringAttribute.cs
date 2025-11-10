using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ERecruitment.Web.Validation;

/// <summary>
/// Validates that a string contains only safe characters (letters, numbers, spaces, and common punctuation).
/// Helps prevent XSS and injection attacks.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public partial class SafeStringAttribute : ValidationAttribute
{
    [GeneratedRegex(@"^[a-zA-Z0-9\s\-'.,&()/]+$")]
    private static partial Regex SafeCharactersRegex();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success; // Use [Required] for required fields
        }

        var stringValue = value.ToString()!;

        if (!SafeCharactersRegex().IsMatch(stringValue))
        {
            return new ValidationResult(
                ErrorMessage ?? $"{validationContext.DisplayName} contains invalid characters");
        }

        return ValidationResult.Success;
    }
}
