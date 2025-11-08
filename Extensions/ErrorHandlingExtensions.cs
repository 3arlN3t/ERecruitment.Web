using System.Collections;
using ERecruitment.Web.Exceptions;
using ERecruitment.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ERecruitment.Web.Extensions;

/// <summary>
/// Extension methods for consistent error handling in controllers.
/// </summary>
public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Creates a BadRequest response for validation errors.
    /// </summary>
    public static BadRequestObjectResult ValidationError(
        this ControllerBase controller,
        string message,
        IDictionary<string, string[]>? validationErrors = null)
    {
        var response = new ErrorResponse(
            message: message,
            errorCode: ErrorCode.ValidationFailed.ToString()
        )
        {
            ValidationErrors = validationErrors as Dictionary<string, string[]>
        };

        return controller.BadRequest(response);
    }

    /// <summary>
    /// Creates a BadRequest response for a single validation error.
    /// </summary>
    public static BadRequestObjectResult ValidationError(
        this ControllerBase controller,
        string fieldName,
        string errorMessage)
    {
        var errors = new Dictionary<string, string[]>
        {
            { fieldName, new[] { errorMessage } }
        };

        return controller.ValidationError("Validation failed", errors);
    }

    /// <summary>
    /// Creates a BadRequest response from ModelState errors.
    /// </summary>
    public static BadRequestObjectResult ValidationError(
        this ControllerBase controller,
        ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new ErrorResponse(
            message: "Validation failed",
            errorCode: ErrorCode.ValidationFailed.ToString()
        )
        {
            ValidationErrors = errors
        };

        return controller.BadRequest(response);
    }

    /// <summary>
    /// Creates a NotFound response.
    /// </summary>
    public static NotFoundObjectResult NotFoundError(
        this ControllerBase controller,
        string resourceType,
        object? resourceId = null)
    {
        var message = resourceId != null
            ? $"{resourceType} with ID '{resourceId}' not found"
            : $"{resourceType} not found";

        var response = new ErrorResponse(
            message: message,
            errorCode: ErrorCode.ResourceNotFound.ToString(),
            details: resourceId != null ? $"ID: {resourceId}" : null
        );

        return controller.NotFound(response);
    }

    /// <summary>
    /// Creates a Conflict response for business rule violations.
    /// </summary>
    public static ConflictObjectResult ConflictError(
        this ControllerBase controller,
        string message,
        string? errorCode = null)
    {
        var response = new ErrorResponse(
            message: message,
            errorCode: errorCode ?? ErrorCode.BusinessRuleViolation.ToString()
        );

        return controller.Conflict(response);
    }

    /// <summary>
    /// Creates a Forbidden response for authorization failures.
    /// </summary>
    public static ForbidResult UnauthorizedError(this ControllerBase controller) =>
        controller.Forbid();

    /// <summary>
    /// Creates a generic error response with custom status code.
    /// </summary>
    public static ObjectResult ErrorResponse(
        this ControllerBase controller,
        int statusCode,
        string message,
        string? errorCode = null,
        string? details = null)
    {
        var response = new ErrorResponse(
            message: message,
            errorCode: errorCode,
            details: details
        );

        return controller.StatusCode(statusCode, response);
    }

    /// <summary>
    /// Safely executes a controller action with error handling.
    /// </summary>
    public static async Task<IActionResult> SafeExecuteAsync(
        this ControllerBase controller,
        Func<Task<IActionResult>> action,
        ILogger logger)
    {
        try
        {
            return await action();
        }
        catch (ResourceNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            return controller.NotFoundError(ex.ResourceType, ex.ResourceId);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            return controller.ValidationError(ex.Message, ex.Errors);
        }
        catch (AuthorizationException ex)
        {
            logger.LogWarning(ex, "Authorization failed: {Message}", ex.Message);
            return controller.Forbid();
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning(ex, "Authentication failed: {Message}", ex.Message);
            return controller.Unauthorized();
        }
        catch (BusinessRuleException ex)
        {
            logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            return controller.ConflictError(ex.Message, ex.ErrorCode);
        }
        catch (ExternalServiceException ex)
        {
            logger.LogError(ex, "External service error: {Message}", ex.Message);
            return controller.ErrorResponse(
                StatusCodes.Status502BadGateway,
                ex.Message,
                ErrorCode.ExternalServiceError.ToString(),
                $"Service: {ex.ServiceName}"
            );
        }
        catch (Exceptions.ApplicationException ex)
        {
            logger.LogError(ex, "Application error: {Message}", ex.Message);
            return controller.BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred");
            return controller.StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("An unexpected error occurred. Please try again later.")
            );
        }
    }

    /// <summary>
    /// Safely executes a controller action with error handling and returns typed result.
    /// </summary>
    public static async Task<IActionResult> SafeExecuteAsync<T>(
        this ControllerBase controller,
        Func<Task<T>> action,
        ILogger logger,
        Func<T, IActionResult>? onSuccess = null)
    {
        try
        {
            var result = await action();
            return onSuccess?.Invoke(result) ?? controller.Ok(result);
        }
        catch (ResourceNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            return controller.NotFoundError(ex.ResourceType, ex.ResourceId);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            return controller.ValidationError(ex.Message, ex.Errors);
        }
        catch (AuthorizationException ex)
        {
            logger.LogWarning(ex, "Authorization failed: {Message}", ex.Message);
            return controller.Forbid();
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning(ex, "Authentication failed: {Message}", ex.Message);
            return controller.Unauthorized();
        }
        catch (BusinessRuleException ex)
        {
            logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
            return controller.ConflictError(ex.Message, ex.ErrorCode);
        }
        catch (ExternalServiceException ex)
        {
            logger.LogError(ex, "External service error: {Message}", ex.Message);
            return controller.ErrorResponse(
                StatusCodes.Status502BadGateway,
                ex.Message,
                ErrorCode.ExternalServiceError.ToString(),
                $"Service: {ex.ServiceName}"
            );
        }
        catch (Exceptions.ApplicationException ex)
        {
            logger.LogError(ex, "Application error: {Message}", ex.Message);
            return controller.BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred");
            return controller.StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("An unexpected error occurred. Please try again later.")
            );
        }
    }
}

