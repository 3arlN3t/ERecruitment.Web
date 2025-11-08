# Error Handling Implementation Checklist

## ✅ System Components (Already Implemented)

- [x] **Global Exception Handling Middleware** (`Middleware/ExceptionHandlingMiddleware.cs`)
  - Catches all unhandled exceptions
  - Converts exceptions to consistent JSON responses
  - Logs exceptions with context
  - Handles different exception types appropriately

- [x] **Custom Exception Classes** (`Exceptions/ApplicationException.cs`)
  - `ApplicationException` - Base exception
  - `ResourceNotFoundException` - Resource not found errors
  - `ValidationException` - Input validation errors
  - `AuthorizationException` - Permission errors
  - `AuthenticationException` - Authentication failures
  - `BusinessRuleException` - Business logic violations
  - `ExternalServiceException` - External service failures

- [x] **Error Response Models** (`Models/ErrorResponse.cs`)
  - `ErrorResponse` - Standard error format
  - `ApiResponse<T>` - Generic API response wrapper
  - `ErrorCode` enum - Standardized error codes

- [x] **Error Handling Extensions** (`Extensions/ErrorHandlingExtensions.cs`)
  - Controller extension methods for consistent error returns
  - `SafeExecuteAsync` wrappers for exception handling
  - ModelState error conversion

- [x] **Error Page/View** (`Views/Shared/Error.cshtml`)
  - Enhanced error display page
  - Status-code-specific error messages
  - Trace ID display for admins
  - Action buttons based on error type

- [x] **Configuration** (`Program.cs`)
  - Middleware registered in pipeline
  - Global exception handling enabled

## 📋 Migration Tasks (For Your Project)

### Phase 1: Service Layer Updates
- [ ] **ApplicantManagementService** - Replace try-catch with exceptions
- [ ] **ApplicationWorkflowService** - Replace try-catch with exceptions
- [ ] **AdministrationService** - Replace try-catch with exceptions
- [ ] **ApplicationExportService** - Replace try-catch with exceptions
- [ ] **EfRecruitmentRepository** - Replace try-catch with exceptions

**Example Pattern:**
```csharp
// Before: Return failed result
if (applicant == null)
    return new RegistrationResult(false, "Applicant not found");

// After: Throw exception
var applicant = ... ?? throw new ResourceNotFoundException("Applicant", id);
```

### Phase 2: Controller Updates

#### High Priority (User-facing operations)
- [ ] **ApplicantController**
  - [ ] `Dashboard()` - Convert try-catch
  - [ ] `Profile()` - Convert try-catch
  - [ ] Application submission methods
  
- [ ] **AdminApplicationsController**
  - [ ] `UpdateStatus()` - Convert try-catch
  - [ ] Download operations
  - [ ] Bulk operations

- [ ] **AdminController**
  - [ ] Login validation
  - [ ] Admin actions

#### Medium Priority
- [ ] **ApplicationsController** - Convert error handling
- [ ] **JobsController** - Convert error handling
- [ ] **AccountController** - Convert error handling
- [ ] **AuditController** - Convert error handling
- [ ] **EmailController** - Convert error handling

### Phase 3: Testing
- [ ] Create unit tests for new exception types
- [ ] Create integration tests for error responses
- [ ] Test HTTP status code mappings
- [ ] Test error message formatting
- [ ] Test validation error serialization

### Phase 4: Documentation
- [ ] Add error handling comments to controllers
- [ ] Document custom business exceptions
- [ ] Create error recovery guides for common scenarios
- [ ] Add troubleshooting guide

## 🔧 Step-by-Step Migration Guide

### Step 1: Update Service Methods

**File:** `Services/ApplicantManagementService.cs`

```csharp
// Before
public async Task<RegistrationResult> RegisterAsync(RegisterDto dto)
{
    try
    {
        if (string.IsNullOrEmpty(dto.Email))
            return new RegistrationResult(false, "Email required");
        
        var applicant = await _repo.GetApplicantByEmailAsync(dto.Email);
        if (applicant != null)
            return new RegistrationResult(false, "Email exists");
        
        // ... more logic
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Registration failed");
        return new RegistrationResult(false, "Registration failed");
    }
}

// After
public async Task<Applicant> RegisterAsync(RegisterDto dto)
{
    // Validate input
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrEmpty(dto.Email))
        errors["Email"] = new[] { "Email is required" };
    
    if (errors.Any())
        throw new ValidationException(errors);
    
    // Check business rules
    var existing = await _repo.GetApplicantByEmailAsync(dto.Email);
    if (existing != null)
        throw new BusinessRuleException(
            "Email already registered",
            errorCode: "EMAIL_EXISTS"
        );
    
    // Proceed with registration
    var applicant = new Applicant { Email = dto.Email };
    await _repo.AddAsync(applicant);
    await _repo.SaveChangesAsync();
    
    return applicant;
}
```

### Step 2: Update Controller Actions

**File:** `Controllers/ApplicantController.cs`

```csharp
// Before
[HttpGet]
public async Task<IActionResult> Profile()
{
    try
    {
        var applicant = await RequireApplicantAsync();
        if (applicant is null)
            return RedirectToAction("Login", "Account");
        
        var model = BuildProfileViewModel(applicant);
        return View("Profile2", model);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load profile");
        throw;
    }
}

// After
[HttpGet]
public async Task<IActionResult> Profile()
{
    return await this.SafeExecuteAsync(
        async () =>
        {
            var applicant = await RequireApplicantAsync()
                ?? throw new AuthenticationException("Applicant profile not found");
            
            var model = BuildProfileViewModel(applicant);
            return View("Profile2", model);
        },
        _logger
    );
}
```

### Step 3: Test Error Handling

Create test file: `Tests/ErrorHandlingTests.cs`

```csharp
[TestFixture]
public class ErrorHandlingTests
{
    [Test]
    public void ResourceNotFoundException_SetsCorrectProperties()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        
        // Act
        var exception = new ResourceNotFoundException("Applicant", resourceId);
        
        // Assert
        Assert.That(exception.ResourceType, Is.EqualTo("Applicant"));
        Assert.That(exception.ResourceId, Is.EqualTo(resourceId));
        Assert.That(exception.Message, Contains.Substring("not found"));
    }

    [Test]
    public void ValidationException_IncludesErrorsInMessage()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required" } },
            { "Password", new[] { "Password must be 8+ characters" } }
        };
        
        // Act
        var exception = new ValidationException(errors);
        
        // Assert
        Assert.That(exception.Errors, Is.EqualTo(errors));
        Assert.That(exception.Message, Contains.Substring("validation"));
    }
}
```

## 📋 Checklist for Each Controller

For each controller that needs updating:

- [ ] Identify all try-catch blocks
- [ ] Determine appropriate exception type for each catch
- [ ] Replace with throw statements
- [ ] Test the new error handling
- [ ] Verify error messages are user-friendly
- [ ] Check logging is appropriate
- [ ] Document any custom exceptions used

### ApplicantController Checklist

- [ ] Dashboard() - No changes needed (no error handling)
- [ ] Profile() GET - Convert exception handling
- [ ] Profile() POST - Convert error handling
- [ ] Application submission - Use SafeExecuteAsync
- [ ] Document download - Add ResourceNotFoundException

### AdminApplicationsController Checklist

- [ ] Index() - Check error handling
- [ ] UpdateStatus() - Convert try-catch
- [ ] ViewProfile() - Add null check with exception
- [ ] DownloadDocument() - Add proper exception handling
- [ ] Bulk operations - Convert error handling

## ✨ Best Practices Checklist

As you implement, verify:

- [ ] All exceptions include meaningful messages
- [ ] Resource IDs are included in ResourceNotFoundException
- [ ] Validation errors are properly formatted
- [ ] Authorization exceptions distinguish between auth and authz
- [ ] External service errors include service name
- [ ] Error codes are defined in ErrorCode enum
- [ ] Sensitive data is not logged in production
- [ ] Trace IDs are included in responses
- [ ] HTTP status codes match exception types
- [ ] User-friendly messages are displayed
- [ ] Admin users can see detailed error info
- [ ] Tests verify error handling paths

## 🧪 Testing Checklist

For each modified method:

- [ ] Test happy path (successful execution)
- [ ] Test validation error path
- [ ] Test resource not found path
- [ ] Test authorization failure path
- [ ] Test unexpected exception path
- [ ] Verify HTTP status code
- [ ] Verify error message format
- [ ] Verify logging occurs

## 📊 Status Tracking

| Component | Status | Priority | Notes |
|-----------|--------|----------|-------|
| Middleware | ✅ Done | - | Global exception handling active |
| Custom Exceptions | ✅ Done | - | All 7 exception types defined |
| Error Models | ✅ Done | - | Standardized response format |
| Extensions | ✅ Done | - | Controller helpers ready |
| Error View | ✅ Done | - | User-friendly error pages |
| Services | ⏳ To Do | High | Needs migration from Result pattern |
| Controllers | ⏳ To Do | High | Needs migration from try-catch |
| Tests | ⏳ To Do | Medium | Comprehensive test coverage |
| Documentation | ✅ Done | - | ERROR_HANDLING_GUIDE.md created |

## 🚀 Deployment Considerations

- [ ] Verify middleware is first in pipeline
- [ ] Test in staging environment
- [ ] Monitor logs for new exception patterns
- [ ] Collect error metrics/analytics
- [ ] Create runbook for common errors
- [ ] Set up alerting for 5xx errors
- [ ] Document support procedures
- [ ] Train support team on new error codes

## 📞 Support Resources

- **Documentation:** `ERROR_HANDLING_GUIDE.md`
- **Examples:** `EXAMPLES_ERROR_HANDLING.md`
- **Quick Reference:** Check `Extensions/ErrorHandlingExtensions.cs` for available methods
- **Exception Types:** See `Exceptions/ApplicationException.cs`
- **Error Codes:** See `Models/ErrorResponse.cs` ErrorCode enum

## 🎯 Goals Achieved

✅ **Consistency** - All errors follow same format  
✅ **Clarity** - Clear error messages and codes  
✅ **Context** - Trace IDs for debugging  
✅ **Security** - Sensitive data protected in production  
✅ **Testability** - Easy to test error scenarios  
✅ **Maintainability** - Centralized error logic  

---

**Last Updated:** 2024  
**Status:** Ready for implementation  
**Next Steps:** Begin Phase 1 - Service layer updates

