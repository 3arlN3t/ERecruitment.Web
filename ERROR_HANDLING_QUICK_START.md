# Error Handling - Quick Start Guide

## 🚀 Getting Started in 5 Minutes

### 1. Verify Installation
```bash
cd /home/ole/Documents/ERecruitment.Web
dotnet build  # Should succeed ✅
```

### 2. Understand the 3 Patterns

#### Pattern A: Service Method Throws
```csharp
// In a service
public async Task<Applicant> GetApplicantAsync(Guid id)
{
    var applicant = await _repo.GetAsync(id);
    if (applicant == null)
        throw new ResourceNotFoundException("Applicant", id);
    return applicant;
}
```

#### Pattern B: Controller Uses SafeExecuteAsync
```csharp
// In a controller
public async Task<IActionResult> GetApplicant(Guid id)
{
    return await this.SafeExecuteAsync(
        async () => {
            var applicant = await _service.GetApplicantAsync(id);
            return Ok(applicant);
        },
        _logger
    );
}
```

#### Pattern C: Controller Uses Helper Methods
```csharp
// In a controller
if (!ModelState.IsValid)
    return this.ValidationError(ModelState);

if (applicant == null)
    return this.NotFoundError("Applicant", id);
```

### 3. Exception Reference

| Use | Exception | Status | Example |
|---|---|---|---|
| Not found | `ResourceNotFoundException` | 404 | `throw new ResourceNotFoundException("Job", id)` |
| Bad input | `ValidationException` | 422 | `throw new ValidationException(errors)` |
| No access | `AuthorizationException` | 403 | `throw new AuthorizationException("No permission")` |
| Need login | `AuthenticationException` | 401 | `throw new AuthenticationException("Login required")` |
| Rule broken | `BusinessRuleException` | 409 | `throw new BusinessRuleException("Closed")` |
| API failed | `ExternalServiceException` | 502 | `throw new ExternalServiceException("Email", "Failed")` |
| Other error | `ApplicationException` | 400 | `throw new ApplicationException("Error")` |

### 4. Add Using Statements

```csharp
using ERecruitment.Web.Exceptions;
using ERecruitment.Web.Extensions;
using ERecruitment.Web.Models;
```

### 5. Migrate One Method

**Before:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetApplicant(Guid id)
{
    try 
    {
        var applicant = await _service.GetApplicant(id);
        if (applicant == null)
            return NotFound();
        return Ok(applicant);
    }
    catch (Exception ex) 
    {
        _logger.LogError(ex, "Error");
        return StatusCode(500);
    }
}
```

**After:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetApplicant(Guid id)
{
    return await this.SafeExecuteAsync(
        async () => {
            var applicant = await _service.GetApplicantAsync(id)
                ?? throw new ResourceNotFoundException("Applicant", id);
            return Ok(applicant);
        },
        _logger
    );
}
```

## 📁 Files to Know

| File | Purpose | When to Use |
|---|---|---|
| `Exceptions/ApplicationException.cs` | Exception types | Reference for what to throw |
| `Extensions/ErrorHandlingExtensions.cs` | Controller helpers | Safe wrappers and error methods |
| `Models/ErrorResponse.cs` | Response models | Understand error format |
| `Middleware/ExceptionHandlingMiddleware.cs` | Global handler | Already configured, just works |
| `Views/Shared/Error.cshtml` | Error page | Already styled, just works |
| `ERROR_HANDLING_GUIDE.md` | Full docs | When you need details |

## 🛠️ Common Tasks

### Task: Add validation to a method
```csharp
var errors = new Dictionary<string, string[]>();

if (string.IsNullOrEmpty(email))
    errors["Email"] = new[] { "Email is required" };

if (errors.Any())
    throw new ValidationException(errors);
```

### Task: Check authorization
```csharp
if (application.ApplicantId != userId)
    throw new AuthorizationException("You cannot access this application");
```

### Task: Check business rule
```csharp
if (job.ClosingDate < DateTime.UtcNow)
    throw new BusinessRuleException("Job posting is closed", errorCode: "JOB_CLOSED");
```

### Task: Call external service safely
```csharp
try {
    await _emailService.SendAsync(email);
} 
catch (Exception ex) {
    throw new ExternalServiceException("EmailService", "Failed to send", ex);
}
```

## ✅ Checklist: Update One Controller

- [ ] Add `using` statements (4 lines)
- [ ] Change one action to use `SafeExecuteAsync`
- [ ] Test it works
- [ ] Do next action in same controller
- [ ] Do next controller

## 🔍 Test Your Implementation

### Test 404
```bash
curl http://localhost:5050/api/applicants/00000000-0000-0000-0000-000000000000
# Should return: {"message": "Applicant not found", ...}
```

### Test Validation Error
```bash
curl -X POST http://localhost:5050/api/applicants \
  -H "Content-Type: application/json" \
  -d '{"email": ""}'
# Should return: {"message": "Validation failed", "validationErrors": ...}
```

### Test Unauthorized
Access admin page without admin role
```bash
curl http://localhost:5050/Admin/Dashboard
# Should redirect to login or show error
```

## 📊 Error Response Format

Every error response has this structure:
```json
{
  "message": "User-friendly message",
  "errorCode": "MACHINE_READABLE_CODE",
  "details": "Optional technical details",
  "validationErrors": {
    "fieldName": ["error 1", "error 2"]
  },
  "traceId": "0HN1GIVGU0EGQ:00000001",
  "timestamp": "2024-01-15T10:30:45.123Z"
}
```

## 🚨 Common Mistakes to Avoid

❌ **Don't:**
```csharp
catch { }  // Never suppress exceptions
throw new Exception("Error");  // Use specific exceptions
return null;  // Throw instead
if (x == null) return null;  // Throw instead
```

✅ **Do:**
```csharp
catch (Exception ex) {
    _logger.LogError(ex, "Error message");
    throw;  // Re-throw or throw custom exception
}
throw new ResourceNotFoundException("Resource", id);
// Let exceptions propagate to middleware
```

## 📈 Implementation Progress

Track your progress:

```
Services to Update: 5
├─ [ ] ApplicantManagementService
├─ [ ] ApplicationWorkflowService  
├─ [ ] AdministrationService
├─ [ ] ApplicationExportService
└─ [ ] EfRecruitmentRepository

Controllers to Update: 8
├─ [ ] ApplicantController
├─ [ ] AdminApplicationsController
├─ [ ] ApplicationsController
├─ [ ] JobsController
├─ [ ] AccountController
├─ [ ] AuditController
├─ [ ] EmailController
└─ [ ] AdminController

Total methods to update: ~100
Estimated time: 6-8 hours
```

## 💡 Pro Tips

1. **Use null coalescing:** `?? throw new ...`
2. **Let middleware handle it:** Just throw exceptions
3. **Log important stuff:** Exceptions in services
4. **Safe wrappers:** Use `SafeExecuteAsync` in controllers
5. **Start simple:** Begin with one controller
6. **Test as you go:** Don't refactor everything at once

## 🔗 More Information

- **Full Guide:** `ERROR_HANDLING_GUIDE.md`
- **Examples:** `EXAMPLES_ERROR_HANDLING.md`
- **Checklist:** `IMPLEMENTATION_CHECKLIST_ERROR_HANDLING.md`
- **Summary:** `ERROR_HANDLING_SUMMARY.md`

## ❓ Troubleshooting

### Problem: Ambiguous reference to ApplicationException
**Solution:** Use `Exceptions.ApplicationException`

### Problem: Method not found
**Solution:** Check namespace imports, might need `using ERecruitment.Web.Extensions;`

### Problem: Exception not caught
**Solution:** Middleware only works for MVC/API exceptions. Check it's registered in Program.cs

### Problem: No trace ID shown
**Solution:** Admin users only see trace ID. Log in as admin.

## 🎯 Next Steps

1. ✅ Verify build works (`dotnet build`)
2. ✅ Read quick reference above
3. 🔄 Pick one controller
4. 🔄 Refactor one method  
5. 🔄 Test it
6. 🔄 Repeat for other methods
7. 📝 Document patterns used
8. ✅ Done!

---

**Time to first result:** 15 minutes  
**Time to refactor one controller:** 30-45 minutes  
**Time to refactor all controllers:** 6-8 hours  
**Difficulty:** Easy to Medium

**Questions?** Check the full guides or look at `EXAMPLES_ERROR_HANDLING.md`

