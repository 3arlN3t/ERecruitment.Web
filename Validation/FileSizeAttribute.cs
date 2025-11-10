using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Validation;

/// <summary>
/// Validates the size of an uploaded file.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class FileSizeAttribute : ValidationAttribute
{
    private readonly long _maxSizeBytes;

    public FileSizeAttribute(long maxSizeMB)
    {
        _maxSizeBytes = maxSizeMB * 1024 * 1024;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Use [Required] for required files
        }

        if (value is IFormFile file && file.Length > _maxSizeBytes)
        {
            var maxSizeMB = _maxSizeBytes / 1024 / 1024;
            return new ValidationResult(
                ErrorMessage ?? $"File size cannot exceed {maxSizeMB} MB");
        }

        return ValidationResult.Success;
    }
}
