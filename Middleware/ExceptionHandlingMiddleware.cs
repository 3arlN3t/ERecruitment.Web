using System.Text.Json;
using ERecruitment.Web.Exceptions;
using ERecruitment.Web.Models;

namespace ERecruitment.Web.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns consistent error responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred. Path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);

            await HandleExceptionAsync(context, exception, _environment);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment environment)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ResourceNotFoundException ex => BuildResourceNotFoundResponse(context, ex),
            ValidationException ex => BuildValidationErrorResponse(context, ex),
            AuthorizationException ex => BuildAuthorizationErrorResponse(context, ex),
            AuthenticationException ex => BuildAuthenticationErrorResponse(context, ex),
            BusinessRuleException ex => BuildBusinessRuleErrorResponse(context, ex),
            ExternalServiceException ex => BuildExternalServiceErrorResponse(context, ex),
            Exceptions.ApplicationException ex => BuildApplicationErrorResponse(context, ex),
            _ => BuildInternalServerErrorResponse(context, exception, environment)
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static ErrorResponse BuildResourceNotFoundResponse(HttpContext context, ResourceNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;

        return new ErrorResponse(
            message: ex.Message,
            errorCode: ErrorCode.ResourceNotFound.ToString(),
            details: $"Resource ID: {ex.ResourceId}"
        )
        {
            TraceId = context.TraceIdentifier
        };
    }

    private static ErrorResponse BuildValidationErrorResponse(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        var response = new ErrorResponse(
            message: ex.Message,
            errorCode: ErrorCode.ValidationFailed.ToString()
        )
        {
            TraceId = context.TraceIdentifier,
            ValidationErrors = ex.Errors.Count > 0
                ? ex.Errors.ToDictionary(x => x.Key, x => x.Value)
                : null
        };

        return response;
    }

    private static ErrorResponse BuildAuthorizationErrorResponse(HttpContext context, AuthorizationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;

        return new ErrorResponse(
            message: ex.Message,
            errorCode: ErrorCode.InsufficientPermissions.ToString(),
            details: $"Required role: {ex.RequiredRole}"
        )
        {
            TraceId = context.TraceIdentifier
        };
    }

    private static ErrorResponse BuildAuthenticationErrorResponse(HttpContext context, AuthenticationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        return new ErrorResponse(
            message: ex.Message,
            errorCode: ErrorCode.Unauthorized.ToString()
        )
        {
            TraceId = context.TraceIdentifier
        };
    }

    private static ErrorResponse BuildBusinessRuleErrorResponse(HttpContext context, BusinessRuleException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;

        return new ErrorResponse(
            message: ex.Message,
            errorCode: ex.ErrorCode ?? ErrorCode.BusinessRuleViolation.ToString()
        )
        {
            TraceId = context.TraceIdentifier
        };
    }

    private static ErrorResponse BuildExternalServiceErrorResponse(HttpContext context, ExternalServiceException ex)
    {
        context.Response.StatusCode = StatusCodes.Status502BadGateway;

        return new ErrorResponse(
            message: ex.Message,
            errorCode: ErrorCode.ExternalServiceError.ToString(),
            details: $"Service: {ex.ServiceName}"
        )
        {
            TraceId = context.TraceIdentifier
        };
    }

    private static ErrorResponse BuildApplicationErrorResponse(HttpContext context, Exceptions.ApplicationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        return new ErrorResponse(
            message: ex.Message,
            errorCode: ErrorCode.BadRequest.ToString()
        )
        {
            TraceId = context.TraceIdentifier
        };
    }

    private static ErrorResponse BuildInternalServerErrorResponse(HttpContext context, Exception exception, IHostEnvironment environment)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var isDevelopment = environment.IsDevelopment();
        var details = isDevelopment
            ? exception.ToString()
            : "An internal server error occurred. Please contact support.";

        return new ErrorResponse(
            message: "An unexpected error occurred",
            errorCode: ErrorCode.InternalServerError.ToString(),
            details: details
        )
        {
            TraceId = context.TraceIdentifier
        };
    }
}

/// <summary>
/// Extension methods for registering exception handling middleware.
/// </summary>
public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

