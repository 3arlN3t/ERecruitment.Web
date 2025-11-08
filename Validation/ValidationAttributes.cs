using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class RequiredWhenAttribute : ValidationAttribute
{
    public string OtherPropertyName { get; }
    public object? ExpectedValue { get; }
    public bool AllowEmptyStrings { get; set; }

    public RequiredWhenAttribute(string otherPropertyName)
    {
        if (string.IsNullOrWhiteSpace(otherPropertyName))
        {
            throw new ArgumentException("Other property name must be provided", nameof(otherPropertyName));
        }

        OtherPropertyName = otherPropertyName;
        ExpectedValue = true;
    }

    public RequiredWhenAttribute(string otherPropertyName, object? expectedValue)
    {
        if (string.IsNullOrWhiteSpace(otherPropertyName))
        {
            throw new ArgumentException("Other property name must be provided", nameof(otherPropertyName));
        }

        OtherPropertyName = otherPropertyName;
        ExpectedValue = expectedValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var otherProperty = validationContext.ObjectType.GetProperty(OtherPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (otherProperty is null)
        {
            return new ValidationResult($"Unknown property {OtherPropertyName}");
        }

        var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

        if (!IsMatch(otherValue))
        {
            return ValidationResult.Success;
        }

        if (IsProvided(value))
        {
            return ValidationResult.Success;
        }

        var displayName = validationContext.DisplayName ?? validationContext.MemberName ?? validationContext.ObjectType.Name;
        var message = ErrorMessage ?? $"{displayName} is required.";
        return new ValidationResult(message, validationContext.MemberName is null ? null : new[] { validationContext.MemberName });
    }

    private bool IsMatch(object? otherValue)
    {
        if (ExpectedValue is null)
        {
            return otherValue is null;
        }

        if (ExpectedValue is bool expectedBool && otherValue is bool actualBool)
        {
            return expectedBool == actualBool;
        }

        return Equals(ExpectedValue, otherValue);
    }

    private bool IsProvided(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is string stringValue)
        {
            return AllowEmptyStrings ? stringValue is not null : !string.IsNullOrWhiteSpace(stringValue);
        }

        if (value is IFormFile file)
        {
            return file.Length > 0;
        }

        if (value is IEnumerable enumerable and not string)
        {
            return enumerable.Cast<object?>().Any(item => item is not null);
        }

        return true;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AtLeastOneRequiredAttribute : ValidationAttribute
{
    public string[] PropertyNames { get; }

    public AtLeastOneRequiredAttribute(params string[] propertyNames)
    {
        if (propertyNames is null || propertyNames.Length == 0)
        {
            throw new ArgumentException("At least one property name must be provided", nameof(propertyNames));
        }

        PropertyNames = propertyNames;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is null)
        {
            return ValidationResult.Success;
        }

        foreach (var propertyName in PropertyNames)
        {
            var property = validationContext.ObjectType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property is null)
            {
                return new ValidationResult($"Unknown property {propertyName}");
            }

            var propertyValue = property.GetValue(validationContext.ObjectInstance);
            if (IsProvided(propertyValue))
            {
                return ValidationResult.Success;
            }
        }

        var displayName = ErrorMessage ?? "At least one field must be provided.";
        return new ValidationResult(displayName, PropertyNames);
    }

    private static bool IsProvided(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue);
        }

        if (value is IEnumerable enumerable and not string)
        {
            return enumerable.Cast<object?>().Any(item => item is not null);
        }

        return true;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class SouthAfricanIdAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string stringValue || string.IsNullOrWhiteSpace(stringValue))
        {
            return ValidationResult.Success;
        }

        var trimmed = stringValue.Trim();

        if (trimmed.Length != 13 || !trimmed.All(char.IsDigit))
        {
            var message = ErrorMessage ?? "South African ID numbers must be 13 digits.";
            return new ValidationResult(message, new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MinCollectionCountAttribute : ValidationAttribute
{
    public int Minimum { get; }

    public MinCollectionCountAttribute(int minimum)
    {
        if (minimum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum must be non-negative");
        }

        Minimum = minimum;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return Minimum == 0
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? $"At least {Minimum} item(s) required.", new[] { validationContext.MemberName! });
        }

        if (value is not IEnumerable enumerable)
        {
            return new ValidationResult("Value must be a collection.", new[] { validationContext.MemberName! });
        }

        var count = enumerable.Cast<object?>().Count();
        if (count >= Minimum)
        {
            return ValidationResult.Success;
        }

        var message = ErrorMessage ?? $"At least {Minimum} item(s) required.";
        return new ValidationResult(message, new[] { validationContext.MemberName! });
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class MustBeTrueAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is bool boolean && boolean)
        {
            return ValidationResult.Success;
        }

        var message = ErrorMessage ?? $"{validationContext.DisplayName ?? validationContext.MemberName} must be accepted.";
        return new ValidationResult(message, validationContext.MemberName is null ? null : new[] { validationContext.MemberName });
    }
}

