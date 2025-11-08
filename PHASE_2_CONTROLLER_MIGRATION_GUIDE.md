# Phase 2: Controller Migration Guide

## Quick Reference - Copy/Paste Templates

### Template 1: Simple Success Redirect
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> YourAction(YourViewModel model)
{
    return await this.SafeExecuteAsync(
        async () => {
            var result = await _service.YourMethodAsync(model);
            TempData["Success"] = "Operation successful!";
            return RedirectToAction("Index");
        },
        _logger
    );
}
```

### Template 2: Return Object Response
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetItem(Guid id)
{
    return await this.SafeExecuteAsync(
        async () => {
            var item = await _service.GetItemAsync(id);
            return Ok(item);
        },
        _logger
    );
}
```

### Template 3: View With Model
```csharp
[HttpGet]
public async Task<IActionResult> Edit(Guid id)
{
    return await this.SafeExecuteAsync(
        async () => {
            var item = await _service.GetItemAsync(id);
            var viewModel = MapToViewModel(item);
            return View(viewModel);
        },
        _logger
    );
}
```

---

## Files Needing Updates

### 1. AccountController.cs
**Location:** Line 91
**Current Code:**
```csharp
var result = await _applicantService.AuthenticateAsync(model);
if (!result.Success)
{
    ModelState.AddModelError("", result.ErrorMessage);
    return View(model);
}
```

**Replacement:** Use SafeExecuteAsync (exceptions handled automatically)

**Methods to Update:**
- [ ] `Login(LoginViewModel model)` - Line ~45-50
- [ ] `Register(RegisterViewModel model)` - Line ~65-75
- [ ] Any other auth methods

---

### 2. ApplicationsController.cs
**Location:** Lines 57, 265
**Current Code:**
```csharp
if (!result.Success)
{
    return BadRequest(result.ErrorMessage);
}
```

**Replacement:** SafeExecuteAsync handles all exceptions

**Methods to Update:**
- [ ] Application submission methods
- [ ] Any methods calling migrated services

---

## Step-by-Step Refactoring Process

### Step 1: Add Using Statement
```csharp
using ERecruitment.Web.Exceptions;
using ERecruitment.Web.Extensions;  // For SafeExecuteAsync
```

### Step 2: Find Old Pattern
```csharp
// OLD PATTERN - Search for these:
var result = await _service.MethodAsync();
if (!result.Success)
{
    // handle error
}
var data = result.Data;  // Access the result
```

### Step 3: Replace with New Pattern
```csharp
// NEW PATTERN
return await this.SafeExecuteAsync(
    async () => {
        var data = await _service.MethodAsync();
        // Use data directly
        return Ok(data);
    },
    _logger
);
```

### Step 4: Test
Run the application and test the endpoint to ensure:
- Success paths work correctly
- Exceptions are caught and return proper error responses
- TempData/flash messages display

---

## Common Patterns to Replace

### Pattern 1: Registration/Login
```csharp
// BEFORE
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    var result = await _applicantService.AuthenticateAsync(model);
    if (!result.Success)
    {
        ModelState.AddModelError("", result.ErrorMessage);
        return View(model);
    }
    
    var applicant = result.Applicant;
    // ... redirect/sign in
}

// AFTER
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    return await this.SafeExecuteAsync(
        async () => {
            var applicant = await _applicantService.AuthenticateAsync(model);
            // ... redirect/sign in
            return RedirectToAction("Dashboard");
        },
        _logger
    );
}
```

### Pattern 2: Update Operations
```csharp
// BEFORE
[HttpPost]
public async Task<IActionResult> Update(UpdateViewModel model)
{
    var result = await _service.UpdateAsync(model);
    if (!result.Success)
    {
        ModelState.AddModelError("", result.ErrorMessage);
        return View(model);
    }
    
    TempData["Success"] = "Updated successfully";
    return RedirectToAction("Index");
}

// AFTER
[HttpPost]
public async Task<IActionResult> Update(UpdateViewModel model)
{
    return await this.SafeExecuteAsync(
        async () => {
            await _service.UpdateAsync(model);
            TempData["Success"] = "Updated successfully";
            return RedirectToAction("Index");
        },
        _logger
    );
}
```

### Pattern 3: API Endpoints
```csharp
// BEFORE
[HttpGet("{id}")]
public async Task<IActionResult> GetItem(Guid id)
{
    var result = await _service.GetItemAsync(id);
    if (!result.Success)
    {
        return NotFound();
    }
    
    return Ok(result.Data);
}

// AFTER
[HttpGet("{id}")]
public async Task<IActionResult> GetItem(Guid id)
{
    return await this.SafeExecuteAsync(
        async () => {
            var item = await _service.GetItemAsync(id);
            return Ok(item);
        },
        _logger
    );
}
```

---

## Files Status Checklist

### AccountController.cs
- [ ] Reviewed file
- [ ] Located all result.Success checks
- [ ] Replaced with SafeExecuteAsync
- [ ] Tested login flow
- [ ] Tested registration flow
- [ ] Verified error responses

### ApplicationsController.cs
- [ ] Reviewed file
- [ ] Located all result.Success checks
- [ ] Replaced with SafeExecuteAsync
- [ ] Tested application submission
- [ ] Tested all workflow methods
- [ ] Verified error responses

### AdminApplicationsController.cs
- [ ] (If using AdministrationService - wait for service migration)

### Other Controllers
- [ ] Identified which use migrated services
- [ ] Updated each one

---

## Verification Checklist

After each controller update:

- [ ] File compiles
- [ ] Test successful operation
- [ ] Test validation error
- [ ] Test not found error
- [ ] Test authorization error
- [ ] Verify error response format
- [ ] Check logs for exceptions
- [ ] Verify TempData messages show

---

## Testing Each Update

### 1. Manual Browser Testing
```
1. Navigate to controller action
2. Test valid input → should succeed
3. Test invalid input → should show error
4. Check browser console for JSON response
5. Verify HTTP status code
```

### 2. Postman/API Testing
```
GET    /api/item/invalid-id
Expected: 404 with ErrorResponse JSON

POST   /api/register (invalid data)
Expected: 422 with ValidationErrors

POST   /api/login (wrong password)
Expected: 401 with error message
```

### 3. Log Verification
```
Check application logs for:
- Exception type and message
- Request path and method
- Trace ID correlation
- Log level (Warning for expected errors, Error for unexpected)
```

---

## Quick Wins - Start With These

### Easiest to Migrate (Start Here)
1. Simple GET endpoints that call services
2. Create/Update forms with redirect
3. Delete operations with redirect

### Moderate Complexity
1. Multi-step workflows
2. Endpoints with multiple service calls
3. Conditional logic based on results

### Most Complex (Do Last)
1. Complex validation scenarios
2. API endpoints with multiple response types
3. Administrative actions

---

## Support Commands

### Find All Result Checks
```bash
grep -n "result\.Success\|result\.ErrorMessage\|result\.Data" Controllers/*.cs
```

### Find All Result Returns
```bash
grep -n "new.*Result(" Controllers/*.cs
```

### Build and Check Errors
```bash
dotnet build 2>&1 | grep error | head -20
```

---

## Rollback Plan

If issues arise:

1. **Revert single file:**
   ```bash
   git checkout Controllers/AccountController.cs
   ```

2. **Revert all controller changes:**
   ```bash
   git checkout Controllers/
   ```

3. **Revert entire Phase 2:**
   ```bash
   git reset --hard <phase-1-commit>
   ```

---

## Success Criteria

✅ **Phase 2 Complete When:**
1. All controllers compile without errors
2. All application flows work end-to-end
3. Error responses have correct format
4. HTTP status codes are appropriate
5. Logs capture all exceptions
6. User-facing error messages are clear

---

## Progress Tracking

Use this table to track your progress:

| File | Status | Tests Passed | Notes |
|------|--------|--------------|-------|
| AccountController.cs | ⏳ | - | - |
| ApplicationsController.cs | ⏳ | - | - |
| AdminApplicationsController.cs | ⏳ | - | - |
| [Other] | ⏳ | - | - |

Update as you complete each file.

---

## Need Help?

1. **Compilation error?** → Check imports, ensure SafeExecuteAsync is available
2. **Wrong HTTP status?** → Check exception mapping in middleware
3. **Missing error details?** → Verify exception has proper message/code
4. **Tests failing?** → Check that service throws correct exception type

Reference the main ERROR_HANDLING_GUIDE.md for complete documentation.

