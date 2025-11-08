using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.Validation;

/// <summary>
/// Validation attribute that requires at least one of two properties to have a non-empty value.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequireEitherAttribute : ValidationAttribute
{
    private readonly string _firstProperty;
    private readonly string _secondProperty;

    public RequireEitherAttribute(string firstProperty, string secondProperty)
    {
        _firstProperty = firstProperty;
        _secondProperty = secondProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var firstValue = validationContext.ObjectType.GetProperty(_firstProperty)?.GetValue(value);
        var secondValue = validationContext.ObjectType.GetProperty(_secondProperty)?.GetValue(value);

        var firstEmpty = string.IsNullOrWhiteSpace(firstValue?.ToString());
        var secondEmpty = string.IsNullOrWhiteSpace(secondValue?.ToString());

        if (firstEmpty && secondEmpty)
        {
            return new ValidationResult(
                ErrorMessage ?? $"Either {_firstProperty} or {_secondProperty} must be provided",
                new[] { _firstProperty, _secondProperty });
        }

        return ValidationResult.Success;
    }
}
