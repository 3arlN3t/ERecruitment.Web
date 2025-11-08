namespace ERecruitment.Web.Models;

/// <summary>
/// Standard error response model for API responses.
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? Details { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }

    public ErrorResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    public ErrorResponse(string message, string? errorCode = null, string? details = null)
        : this()
    {
        Message = message;
        ErrorCode = errorCode;
        Details = details;
    }
}

/// <summary>
/// Generic API response model for wrapped responses.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ErrorResponse? Error { get; set; }
    public DateTime Timestamp { get; set; }

    public ApiResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, string? errorCode = null, string? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ErrorResponse(message, errorCode, details)
        };
    }

    public static ApiResponse<T> ErrorResponse(ErrorResponse error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}

/// <summary>
/// Enum for standardized error codes.
/// </summary>
public enum ErrorCode
{
    // General
    InternalServerError = 500,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    UnprocessableEntity = 422,

    // Validation
    ValidationFailed = 4001,
    InvalidInput = 4002,

    // Authentication & Authorization
    InvalidCredentials = 4011,
    TokenExpired = 4012,
    InsufficientPermissions = 4031,

    // Resource
    ResourceNotFound = 4041,
    ResourceAlreadyExists = 4042,

    // Business Logic
    InvalidOperation = 4091,
    BusinessRuleViolation = 4092,

    // External Services
    ExternalServiceError = 5021,
    ExternalServiceUnavailable = 5022
}

