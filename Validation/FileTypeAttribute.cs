using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Validation;

/// <summary>
/// Validates the file extension of an uploaded file.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class FileTypeAttribute : ValidationAttribute
{
    private readonly string[] _allowedExtensions;

    public FileTypeAttribute(params string[] allowedExtensions)
    {
        _allowedExtensions = allowedExtensions.Select(ext => ext.ToLowerInvariant()).ToArray();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Use [Required] for required files
        }

        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                return new ValidationResult(
                    ErrorMessage ?? $"Only {string.Join(", ", _allowedExtensions)} files are allowed");
            }
        }

        return ValidationResult.Success;
    }
}
