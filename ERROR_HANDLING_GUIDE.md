# Error Handling Implementation Guide

## Overview

This document describes the comprehensive error handling system implemented in the ERecruitment.Web application. The system provides:

- **Global exception handling middleware** for catching all unhandled exceptions
- **Custom exception types** for different error scenarios
- **Consistent error response models** for API and MVC responses
- **Standardized error codes** for client-side handling
- **Detailed error logging** for monitoring and debugging

## Architecture

### Components

1. **ExceptionHandlingMiddleware** (`Middleware/ExceptionHandlingMiddleware.cs`)
   - Catches all unhandled exceptions in the request pipeline
   - Converts exceptions to consistent JSON error responses
   - Logs all exceptions with context information
   - Handles different exception types with appropriate HTTP status codes

2. **Custom Exception Classes** (`Exceptions/ApplicationException.cs`)
   - `ApplicationException` - Base exception for all application errors
   - `ResourceNotFoundException` - When a requested resource doesn't exist
   - `ValidationException` - When input validation fails
   - `AuthorizationException` - When a user lacks permissions
   - `AuthenticationException` - When authentication fails
   - `BusinessRuleException` - When business logic constraints are violated
   - `ExternalServiceException` - When external services fail

3. **Error Response Models** (`Models/ErrorResponse.cs`)
   - `ErrorResponse` - Standard error response with details
   - `ApiResponse<T>` - Generic wrapper for API responses
   - `ErrorCode` enum - Standardized error codes

4. **Error Handling Extensions** (`Extensions/ErrorHandlingExtensions.cs`)
   - Helper methods for controllers to return consistent errors
   - Safe execution wrappers for exception handling
   - Model state error conversion

## Usage Guide

### 1. In Services/Business Logic - Throwing Exceptions

```csharp
// Resource not found
if (applicant == null)
{
    throw new ResourceNotFoundException("Applicant", applicantId);
}

// Validation error
if (string.IsNullOrWhiteSpace(model.Email))
{
    var errors = new Dictionary<string, string[]>
    {
        { "Email", new[] { "Email is required" } }
    };
    throw new ValidationException(errors);
}

// Authorization error
if (!user.HasRole("Admin"))
{
    throw new AuthorizationException(
        "User does not have admin privileges", 
        requiredRole: "Admin"
    );
}

// Business rule violation
if (job.ClosingDate < DateTime.UtcNow)
{
    throw new BusinessRuleException(
        "Cannot apply to a job with a passed closing date",
        errorCode: "JOB_CLOSED"
    );
}

// External service error
try
{
    await emailService.SendAsync(email);
}
catch (Exception ex)
{
    throw new ExternalServiceException("EmailService", "Failed to send email", ex);
}
```

### 2. In Controllers - Handling Exceptions

#### Method 1: Using SafeExecuteAsync (Recommended)

```csharp
public async Task<IActionResult> GetApplicant(Guid id)
{
    return await this.SafeExecuteAsync(
        async () => 
        {
            var applicant = await _service.GetApplicantAsync(id);
            return Ok(applicant);
        },
        _logger
    );
}
```

#### Method 2: Using Dedicated Extension Methods

```csharp
public IActionResult GetApplicant(Guid id)
{
    try
    {
        var applicant = _service.GetApplicant(id);
        if (applicant == null)
            return this.NotFoundError("Applicant", id);
        
        return Ok(applicant);
    }
    catch (ValidationException ex)
    {
        return this.ValidationError("Validation failed", ex.Errors);
    }
    catch (BusinessRuleException ex)
    {
        return this.ConflictError(ex.Message, ex.ErrorCode);
    }
}
```

#### Method 3: Let Global Middleware Handle It

Simply throw exceptions - the middleware will catch and format them:

```csharp
public async Task<IActionResult> GetApplicant(Guid id)
{
    var applicant = await _service.GetApplicantAsync(id) 
        ?? throw new ResourceNotFoundException("Applicant", id);
    
    return Ok(applicant);
}
```

### 3. Available Extension Methods for Controllers

```csharp
// Validation errors
controller.ValidationError("Validation failed", errors);
controller.ValidationError("Email", "Email is required");
controller.ValidationError(ModelState);

// Not found
controller.NotFoundError("Applicant", applicantId);

// Business rule violations
controller.ConflictError("Cannot perform action", "ERROR_CODE");

// Authorization
controller.UnauthorizedError();

// Generic error with custom status code
controller.ErrorResponse(500, "Error message", "ERROR_CODE", "Additional details");
```

## Error Codes

Standard error codes used throughout the application:

### General (4xx-5xx)
- `500` - InternalServerError
- `400` - BadRequest
- `401` - Unauthorized
- `403` - Forbidden
- `404` - NotFound
- `409` - Conflict
- `422` - UnprocessableEntity

### Validation (4001-4002)
- `4001` - ValidationFailed
- `4002` - InvalidInput

### Authentication & Authorization (4011-4031)
- `4011` - InvalidCredentials
- `4012` - TokenExpired
- `4031` - InsufficientPermissions

### Resource (4041-4042)
- `4041` - ResourceNotFound
- `4042` - ResourceAlreadyExists

### Business Logic (4091-4092)
- `4091` - InvalidOperation
- `4092` - BusinessRuleViolation

### External Services (5021-5022)
- `5021` - ExternalServiceError
- `5022` - ExternalServiceUnavailable

## Error Response Format

All errors return a consistent JSON format:

```json
{
  "message": "Applicant not found.",
  "errorCode": "ResourceNotFound",
  "details": "Resource ID: 550e8400-e29b-41d4-a716-446655440000",
  "validationErrors": null,
  "traceId": "0HN1GIVGU0EGQ:00000001",
  "timestamp": "2024-01-15T10:30:45.123Z"
}
```

Validation error response example:

```json
{
  "message": "Validation failed",
  "errorCode": "ValidationFailed",
  "validationErrors": {
    "Email": ["Email is required", "Invalid email format"],
    "Password": ["Password must be at least 8 characters"]
  },
  "traceId": "0HN1GIVGU0EGQ:00000002",
  "timestamp": "2024-01-15T10:30:46.456Z"
}
```

## HTTP Status Code Mapping

| Exception Type | HTTP Status | Error Code |
|---|---|---|
| ResourceNotFoundException | 404 | ResourceNotFound |
| ValidationException | 422 | ValidationFailed |
| AuthorizationException | 403 | InsufficientPermissions |
| AuthenticationException | 401 | Unauthorized |
| BusinessRuleException | 409 | BusinessRuleViolation |
| ExternalServiceException | 502 | ExternalServiceError |
| ApplicationException | 400 | BadRequest |
| Other Exception | 500 | InternalServerError |

## Logging

All exceptions are automatically logged by the middleware with the following information:
- Exception type and message
- Stack trace (in development)
- Request path and method
- User information (if authenticated)
- Trace ID for correlation

```csharp
_logger.LogError(exception, 
    "Unhandled exception occurred. Path: {Path}, Method: {Method}",
    context.Request.Path, context.Request.Method);
```

## Best Practices

### 1. Use Specific Exceptions
```csharp
// Good - Specific exception
throw new ResourceNotFoundException("JobPosting", jobId);

// Bad - Generic exception
throw new Exception("Job not found");
```

### 2. Include Context Information
```csharp
// Good - Includes resource identifier
throw new ResourceNotFoundException("Applicant", applicantId);

// Less helpful
throw new ResourceNotFoundException("Applicant");
```

### 3. Use Error Codes for Business Rules
```csharp
// Good - Enables client-side error handling
throw new BusinessRuleException(
    "Application deadline has passed",
    errorCode: "APPLICATION_DEADLINE_PASSED"
);

// Less helpful
throw new BusinessRuleException("Cannot apply to this job");
```

### 4. Log and Throw vs. Just Throw
```csharp
// Good - Exception message is clear
throw new ValidationException(new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required" } }
});

// Service layer logs, controller throws
catch (Exception ex)
{
    _logger.LogError(ex, "Service operation failed");
    throw new ExternalServiceException("PaymentService", "Payment processing failed", ex);
}
```

### 5. Don't Catch and Suppress
```csharp
// Bad - Hides errors
try 
{ 
    await _service.DoSomething(); 
}
catch { }  // ❌ Never do this

// Good - Re-throw or wrap appropriately
try 
{ 
    await _service.DoSomething(); 
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;  // Re-throw for middleware to handle
}
```

## Testing Error Handling

### Testing Exception Throwing
```csharp
[Test]
public async Task GetApplicant_WhenNotFound_ThrowsNotFoundException()
{
    // Arrange
    var id = Guid.NewGuid();
    _mockService.Setup(s => s.GetApplicantAsync(id))
        .ReturnsAsync((Applicant)null);

    // Act & Assert
    Assert.ThrowsAsync<ResourceNotFoundException>(
        () => _controller.GetApplicant(id)
    );
}
```

### Testing Error Responses
```csharp
[Test]
public async Task GetApplicant_ReturnsNotFound_WhenApplicantDoesNotExist()
{
    // Arrange
    _mockService.Setup(s => s.GetApplicantAsync(It.IsAny<Guid>()))
        .ThrowsAsync(new ResourceNotFoundException("Applicant"));

    // Act
    var result = await _controller.GetApplicant(Guid.NewGuid());

    // Assert
    Assert.IsInstanceOf<NotFoundObjectResult>(result);
    var notFoundResult = result as NotFoundObjectResult;
    Assert.AreEqual(404, notFoundResult.StatusCode);
}
```

## Configuration

### Development vs Production

In **Development**:
- Detailed error messages with stack traces
- Full exception details in responses
- Sensitive data logging enabled

In **Production**:
- Generic error messages
- Stack traces hidden
- Sensitive data not logged

Control via environment:
```csharp
var details = _environment.IsDevelopment()
    ? exception.ToString()
    : "An internal server error occurred. Please contact support.";
```

## Migration Guide

### Updating Existing Controllers

**Before:**
```csharp
public IActionResult GetApplicant(Guid id)
{
    try
    {
        var applicant = _service.GetApplicant(id);
        if (applicant == null)
            return NotFound();
        
        return Ok(applicant);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting applicant");
        return StatusCode(500);
    }
}
```

**After:**
```csharp
public async Task<IActionResult> GetApplicant(Guid id)
{
    return await this.SafeExecuteAsync(
        async () => 
        {
            var applicant = await _service.GetApplicantAsync(id)
                ?? throw new ResourceNotFoundException("Applicant", id);
            return Ok(applicant);
        },
        _logger
    );
}
```

## Common Scenarios

### Scenario 1: Validating User Input
```csharp
if (!ModelState.IsValid)
{
    throw new ValidationException(
        ModelState
            .Where(x => x.Value.Errors.Any())
            .ToDictionary(
                x => x.Key,
                x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            )
    );
}
```

### Scenario 2: Authorization Check
```csharp
if (!await _authService.CanUserAccessApplicantAsync(userId, applicantId))
{
    throw new AuthorizationException(
        "User does not have permission to access this applicant"
    );
}
```

### Scenario 3: Optional Resource with Fallback
```csharp
var applicant = await _service.GetApplicantAsync(id);
if (applicant == null)
{
    _logger.LogWarning("Applicant {ApplicantId} not found, returning default", id);
    return Ok(GetDefaultApplicant());
}

return Ok(applicant);
```

## Summary

The error handling system provides:

✅ **Consistency** - All errors follow the same format  
✅ **Clarity** - Clear error messages and codes  
✅ **Context** - Trace IDs for debugging  
✅ **Security** - Sensitive data hidden in production  
✅ **Testability** - Easy to test error scenarios  
✅ **Maintainability** - Centralized error handling logic  

Use these patterns throughout the application for robust, professional error handling.

