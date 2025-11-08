# Services Migration Progress - Exception-Based Error Handling

## Overview

This document tracks the migration of all services from returning `Result` objects to throwing exceptions for consistent error handling.

## Completed ✅

### 1. ApplicantManagementService
- [x] Added `using ERecruitment.Web.Exceptions;`
- [x] Refactored `RegisterApplicantAsync()` - Returns `Applicant` instead of `RegistrationResult`
  - Throws `ValidationException` for input errors
  - Throws `BusinessRuleException` for duplicate email
- [x] Refactored `AuthenticateAsync()` - Returns `Applicant` instead of `AuthenticationResult`
  - Throws `AuthenticationException` for invalid credentials
- [x] Refactored `UpdateProfileAsync()` - Returns `void` instead of `ProfileUpdateResult`
  - Throws `ResourceNotFoundException` if applicant null
- [x] Updated `IApplicantManagementService` interface with new signatures and documentation

### 2. ApplicationWorkflowService (Partial)
- [x] Added `using ERecruitment.Web.Exceptions;`
- [x] Refactored `StartApplicationAsync()` - Returns `JobApplication` instead of `ApplicationFlowResult`
  - Throws `ResourceNotFoundException` if job not found
  - Throws `BusinessRuleException` if job expired or inactive
- [x] Refactored `SubmitDirectApplicationAsync()` - Returns `JobApplication` instead of `ApplicationFlowResult`
  - Throws `BusinessRuleException` for all invalid states
- [x] Updated `IApplicationWorkflowService` interface with new signatures
- [ ] Refactor remaining methods (SubmitKillerQuestionAsync, SubmitScreenedApplicationAsync, WithdrawApplicationAsync)

## In Progress 🔄

### ApplicationWorkflowService - Remaining Methods

**SubmitKillerQuestionAsync** - Needs refactoring:
```csharp
// BEFORE:
public async Task<ApplicationFlowResult> SubmitKillerQuestionAsync(...)
{
    if (job is null) return new ApplicationFlowResult(false, "Job not found.");
    // ... many return statements
}

// AFTER:
public async Task<JobApplication> SubmitKillerQuestionAsync(...)
{
    var job = await _repository.GetJobPostingAsync(jobId, cancellationToken)
        ?? throw new ResourceNotFoundException("JobPosting", jobId);
    // ... throw BusinessRuleException for invalid states
}
```

**SubmitScreenedApplicationAsync** - Needs refactoring (similar pattern)

**WithdrawApplicationAsync** - Needs refactoring (similar pattern)

## TODO 📋

### 3. AdministrationService
Location: `/Services/AdministrationService.cs`

Methods to refactor:
- [x] Import exceptions
- [ ] `BulkRejectApplicationsAsync()` - Currently returns `BulkRejectResult`
  - Should throw `ValidationException` if no applications selected
  - Should throw `ResourceNotFoundException` if applications not found
  - Should return `int` (count of updated records)

- [ ] `UpdateApplicationStatusAsync()` - Currently returns `ApplicationStatusUpdateResult`
  - Should throw exceptions instead of returning failures
  - Return type should be `Task` (void)

Example refactoring for `BulkRejectApplicationsAsync`:
```csharp
// BEFORE
public async Task<BulkRejectResult> BulkRejectApplicationsAsync(
    AdminBulkRejectViewModel model,
    CancellationToken cancellationToken = default)
{
    if (model.SelectedApplicationIds.Count == 0)
    {
        return new BulkRejectResult(0, Array.Empty<AuditEntry>());
    }
    // ...
}

// AFTER
public async Task<int> BulkRejectApplicationsAsync(
    AdminBulkRejectViewModel model,
    CancellationToken cancellationToken = default)
{
    if (model.SelectedApplicationIds.Count == 0)
    {
        throw new ValidationException(new Dictionary<string, string[]>
        {
            { "SelectedApplicationIds", new[] { "At least one application must be selected." } }
        });
    }
    // ...
    return count;
}
```

### 4. ApplicationExportService
Location: `/Services/ApplicationExportService.cs`

Methods to refactor:
- [ ] `ExportApplicationsCsvAsync()` - Currently returns `FileDownloadResult`
  - Should throw `BusinessRuleException` if no applications to export
  - Return type should be `Task<byte[]>` or `Task<FileDownloadResult>` (keep as-is if simple result)

### 5. Legacy ApplicationService (if used)
Location: `/Services/ApplicationService.cs`

This appears to be a legacy implementation. Check if still used:
- [ ] If used: Refactor all Result-returning methods
- [ ] If deprecated: Remove or mark as obsolete

## Refactoring Patterns

### Pattern 1: Validation Error
```csharp
// Before
if (string.IsNullOrWhiteSpace(input))
    return new Result(false, "Input required");

// After
if (string.IsNullOrWhiteSpace(input))
    throw new ValidationException(new Dictionary<string, string[]>
    {
        { "input", new[] { "Input is required" } }
    });
```

### Pattern 2: Resource Not Found
```csharp
// Before
var item = await _repo.GetAsync(id);
if (item is null)
    return new Result(false, "Item not found");

// After
var item = await _repo.GetAsync(id)
    ?? throw new ResourceNotFoundException("Item", id);
```

### Pattern 3: Business Rule Violation
```csharp
// Before
if (status == InvalidStatus)
    return new Result(false, "Cannot transition to this state");

// After
if (status == InvalidStatus)
    throw new BusinessRuleException(
        "Cannot transition to this state",
        errorCode: "INVALID_STATE_TRANSITION");
```

### Pattern 4: Multiple Error Returns
```csharp
// Before
if (errorA)
    return new Result(false, "Error A");
if (errorB)
    return new Result(false, "Error B");
if (errorC)
    return new Result(false, "Error C");

// After
var errors = new Dictionary<string, string[]>();
if (errorA) errors["field1"] = new[] { "Error A" };
if (errorB) errors["field2"] = new[] { "Error B" };
if (errorC) errors["field3"] = new[] { "Error C" };
if (errors.Any()) throw new ValidationException(errors);
```

## Interface Changes Summary

| Service | Old Interface | New Interface | Return Type Changes |
|---------|---------------|---------------|-------------------|
| IApplicantManagementService | RegistrationResult | Applicant | ✅ Changed, throws exceptions |
| IApplicantManagementService | AuthenticationResult | Applicant | ✅ Changed, throws exceptions |
| IApplicantManagementService | ProfileUpdateResult | void | ✅ Changed, throws exceptions |
| IApplicationWorkflowService | ApplicationFlowResult | JobApplication | ✅ Partial, WIP |
| IAdministrationService | ApplicationStatusUpdateResult | void | ⏳ TODO |
| IAdministrationService | BulkRejectResult | int | ⏳ TODO |
| IApplicationExportService | FileDownloadResult | * | ⏳ TODO |

## Files Modified

### ✅ Completed
- `Services/ApplicantManagementService.cs` - Full refactor done

### 🔄 In Progress
- `Services/ApplicationWorkflowService.cs` - Partial (2/5 methods, interface updated)

### ⏳ Pending
- `Services/AdministrationService.cs`
- `Services/ApplicationExportService.cs`
- `Services/ApplicationService.cs` (if still used)

## Controller Updates Required

After all services are refactored, update controllers to handle exceptions:

### ApplicantController changes needed:
```csharp
// Old pattern - still checking result.Success
var result = await _service.RegisterApplicantAsync(model);
if (!result.Success)
    return BadRequest(result.ErrorMessage);

// New pattern - using SafeExecuteAsync
return await this.SafeExecuteAsync(
    async () => {
        var applicant = await _service.RegisterApplicantAsync(model);
        return Ok(applicant);
    },
    _logger
);
```

### AccountController changes needed:
- Refactor to use new exception-based authentication

### AdminApplicationsController changes needed:
- Refactor to handle new AdministrationService exceptions

### AdminController changes needed:
- Refactor if uses any migrated services

## Testing Strategy

For each refactored service:

1. **Unit Tests** - Verify exceptions thrown correctly
   ```csharp
   [Test]
   public async Task RegisterAsync_WithDuplicateEmail_ThrowsBusinessRuleException()
   {
       Assert.ThrowsAsync<BusinessRuleException>(
           () => _service.RegisterApplicantAsync(model)
       );
   }
   ```

2. **Integration Tests** - Verify controller handling
   ```csharp
   [Test]
   public async Task RegisterPost_WithValidData_Returns200()
   {
       var result = await _controller.Register(model);
       Assert.IsInstanceOf<OkResult>(result);
   }
   ```

3. **Error Response Tests** - Verify middleware handling
   ```csharp
   [Test]
   public async Task MissingResource_Returns404()
   {
       var response = await _client.GetAsync("/api/applicants/invalid-id");
       Assert.AreEqual(404, response.StatusCode);
   }
   ```

## Estimated Remaining Work

| Task | Estimated Time | Priority |
|------|---|---|
| ApplicationWorkflowService (remaining methods) | 2-3 hours | HIGH |
| AdministrationService | 1-2 hours | HIGH |
| ApplicationExportService | 30 min | MEDIUM |
| ApplicationService (if used) | 1-2 hours | LOW |
| Controller updates (Phase 2) | 4-6 hours | HIGH |
| Testing | 2-3 hours | HIGH |
| **TOTAL** | **11-18 hours** | - |

## Migration Checklist

- [x] Phase 1a: ApplicantManagementService refactored
- [x] Phase 1b: ApplicationWorkflowService (partial)
- [ ] Phase 1c: ApplicationWorkflowService (complete)
- [ ] Phase 1d: AdministrationService
- [ ] Phase 1e: ApplicationExportService
- [ ] Phase 1f: Legacy ApplicationService (if needed)
- [ ] Phase 2: Controller updates
- [ ] Phase 3: Test coverage
- [ ] Phase 4: Verification & deployment

## Next Steps

1. Complete ApplicationWorkflowService remaining methods (SubmitKillerQuestionAsync, SubmitScreenedApplicationAsync, WithdrawApplicationAsync)
2. Refactor AdministrationService
3. Handle ApplicationExportService
4. Update all controllers to use new exception-based services
5. Create comprehensive tests
6. Verify error responses end-to-end
7. Deploy and monitor

---

**Last Updated:** November 2024  
**Status:** In Progress (30% complete)  
**Owner:** Development Team

