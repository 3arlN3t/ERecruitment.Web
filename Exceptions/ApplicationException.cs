namespace ERecruitment.Web.Exceptions;

/// <summary>
/// Base exception for application-specific errors.
/// </summary>
public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message)
    {
    }

    public ApplicationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception raised when a requested resource is not found.
/// </summary>
public class ResourceNotFoundException : ApplicationException
{
    public string ResourceType { get; }
    public object? ResourceId { get; }

    public ResourceNotFoundException(string resourceType, object? resourceId = null) 
        : base($"{resourceType} not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public ResourceNotFoundException(string message, string resourceType, object? resourceId = null) 
        : base(message)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception raised when a validation fails.
/// </summary>
public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception raised when a user is not authorized to perform an action.
/// </summary>
public class AuthorizationException : ApplicationException
{
    public string? RequiredRole { get; }

    public AuthorizationException(string message, string? requiredRole = null) 
        : base(message)
    {
        RequiredRole = requiredRole;
    }
}

/// <summary>
/// Exception raised when authentication fails.
/// </summary>
public class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception raised when an operation cannot be performed due to business rules.
/// </summary>
public class BusinessRuleException : ApplicationException
{
    public string? ErrorCode { get; }

    public BusinessRuleException(string message, string? errorCode = null) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception raised when an external service fails.
/// </summary>
public class ExternalServiceException : ApplicationException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message) 
        : base($"External service '{serviceName}' error: {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base($"External service '{serviceName}' error: {message}", innerException)
    {
        ServiceName = serviceName;
    }
}

