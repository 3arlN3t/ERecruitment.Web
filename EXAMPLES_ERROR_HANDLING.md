# Error Handling Implementation Examples

This document provides practical examples of how to use the error handling system in different scenarios.

## Table of Contents

1. [Service Layer Examples](#service-layer-examples)
2. [Controller Examples](#controller-examples)
3. [Common Patterns](#common-patterns)
4. [Refactoring Examples](#refactoring-examples)

## Service Layer Examples

### Example 1: Retrieving a Resource

```csharp
public class ApplicantService : IApplicantService
{
    private readonly IRepository _repository;
    private readonly ILogger<ApplicantService> _logger;

    public async Task<Applicant> GetApplicantAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "id", new[] { "Applicant ID cannot be empty" } }
            });
        }

        var applicant = await _repository.GetApplicantAsync(id);
        
        if (applicant == null)
        {
            _logger.LogWarning("Applicant {ApplicantId} not found", id);
            throw new ResourceNotFoundException("Applicant", id);
        }

        return applicant;
    }
}
```

### Example 2: Creating a Resource with Validation

```csharp
public async Task<Applicant> CreateApplicantAsync(CreateApplicantDto dto)
{
    // Validate input
    var validationErrors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(dto.Email))
    {
        validationErrors["Email"] = new[] { "Email is required" };
    }
    else if (!IsValidEmail(dto.Email))
    {
        validationErrors["Email"] = new[] { "Email format is invalid" };
    }

    if (string.IsNullOrWhiteSpace(dto.FirstName))
    {
        validationErrors["FirstName"] = new[] { "First name is required" };
    }

    if (validationErrors.Any())
    {
        throw new ValidationException(validationErrors);
    }

    // Check for duplicates
    if (await _repository.ApplicantExistsAsync(dto.Email))
    {
        throw new BusinessRuleException(
            "An applicant with this email already exists",
            errorCode: "APPLICANT_EMAIL_EXISTS"
        );
    }

    // Create the applicant
    var applicant = new Applicant(dto.Email, dto.FirstName, dto.LastName);
    await _repository.AddAsync(applicant);
    await _repository.SaveChangesAsync();

    _logger.LogInformation("Applicant {ApplicantId} created successfully", applicant.Id);

    return applicant;
}
```

### Example 3: Authorization Check

```csharp
public async Task<JobApplication> UpdateApplicationAsync(
    Guid applicationId,
    UpdateApplicationDto dto,
    string userId)
{
    var application = await _repository.GetApplicationAsync(applicationId)
        ?? throw new ResourceNotFoundException("JobApplication", applicationId);

    // Check authorization
    if (application.Applicant.UserId != userId)
    {
        _logger.LogWarning(
            "User {UserId} attempted to update application {ApplicationId} they don't own",
            userId, applicationId
        );
        throw new AuthorizationException(
            "You do not have permission to update this application"
        );
    }

    // Check business rules
    if (application.Status == ApplicationStatus.Rejected)
    {
        throw new BusinessRuleException(
            "Cannot update a rejected application",
            errorCode: "APPLICATION_REJECTED"
        );
    }

    application.Update(dto.CoverLetter, dto.AdditionalInfo);
    await _repository.SaveChangesAsync();

    return application;
}
```

### Example 4: External Service Call

```csharp
public async Task SendApplicationNotificationAsync(
    JobApplication application,
    string subject)
{
    try
    {
        var emailBody = await RenderEmailTemplateAsync(application);
        
        var email = new Email
        {
            To = application.Applicant.User.Email,
            Subject = subject,
            Body = emailBody
        };

        await _emailService.SendAsync(email);
        
        _logger.LogInformation(
            "Application notification sent to {Email}",
            application.Applicant.User.Email
        );
    }
    catch (SmtpException ex)
    {
        throw new ExternalServiceException(
            "EmailService",
            "Failed to send application notification email",
            ex
        );
    }
    catch (Exception ex)
    {
        throw new ExternalServiceException(
            "EmailService",
            "Unexpected error while sending email notification",
            ex
        );
    }
}
```

## Controller Examples

### Example 1: Basic CRUD Operations

```csharp
[ApiController]
[Route("api/[controller]")]
public class ApplicantsController : ControllerBase
{
    private readonly IApplicantService _service;
    private readonly ILogger<ApplicantsController> _logger;

    // GET /api/applicants/{id}
    [HttpGet("{id}")]
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

    // POST /api/applicants
    [HttpPost]
    public async Task<IActionResult> CreateApplicant(CreateApplicantDto dto)
    {
        return await this.SafeExecuteAsync(
            async () => 
            {
                var applicant = await _service.CreateApplicantAsync(dto);
                return CreatedAtAction(nameof(GetApplicant), 
                    new { id = applicant.Id }, applicant);
            },
            _logger
        );
    }

    // PUT /api/applicants/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateApplicant(Guid id, UpdateApplicantDto dto)
    {
        return await this.SafeExecuteAsync(
            async () => 
            {
                var applicant = await _service.UpdateApplicantAsync(id, dto);
                return Ok(applicant);
            },
            _logger
        );
    }

    // DELETE /api/applicants/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteApplicant(Guid id)
    {
        return await this.SafeExecuteAsync(
            async () => 
            {
                await _service.DeleteApplicantAsync(id);
                return NoContent();
            },
            _logger
        );
    }
}
```

### Example 2: Complex Business Logic

```csharp
[Authorize]
public class ApplicationsController : Controller
{
    private readonly IApplicationService _service;
    private readonly ILogger<ApplicationsController> _logger;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitApplicationDto dto)
    {
        return await this.SafeExecuteAsync(
            async () => 
            {
                // Get current user's applicant record
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var applicant = await _service.GetApplicantByUserIdAsync(userId)
                    ?? throw new AuthenticationException(
                        "Applicant profile not found for current user"
                    );

                // Validate form
                if (!ModelState.IsValid)
                {
                    throw new ValidationException(ModelState.Values
                        .Where(v => v.Errors.Any())
                        .ToDictionary(
                            v => v.ToString(),
                            v => v.Errors.Select(e => e.ErrorMessage).ToArray()
                        ));
                }

                // Submit application
                var application = await _service.SubmitApplicationAsync(
                    applicant.Id,
                    dto.JobPostingId,
                    dto.CoverLetter
                );

                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction("Details", new { id = application.Id });
            },
            _logger
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(Guid id)
    {
        return await this.SafeExecuteAsync(
            async () => 
            {
                var application = await _service.WithdrawApplicationAsync(id);
                TempData["Success"] = "Application withdrawn successfully.";
                return RedirectToAction("Index");
            },
            _logger
        );
    }
}
```

### Example 3: With Custom Success Handling

```csharp
[HttpPost]
public async Task<IActionResult> ImportApplicants(IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        return this.ValidationError("File", "Please select a file to import");
    }

    if (file.ContentType != "text/csv")
    {
        return this.ValidationError(
            new Dictionary<string, string[]>
            {
                { "File", new[] { "Only CSV files are supported" } }
            }
        );
    }

    return await this.SafeExecuteAsync(
        async () => await _service.ImportApplicantsAsync(file),
        _logger,
        onSuccess: result => Ok(new
        {
            message = $"Successfully imported {result.ImportedCount} applicants",
            importedCount = result.ImportedCount,
            failedCount = result.FailedCount,
            errors = result.Errors
        })
    );
}
```

## Common Patterns

### Pattern 1: Null Coalescing with Exception

```csharp
// Instead of null checks scattered throughout
public async Task<IActionResult> GetApplicant(Guid id)
{
    var applicant = await _service.GetApplicantAsync(id);
    if (applicant == null)
        return NotFound();
    
    return Ok(applicant);
}

// Use this pattern
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

### Pattern 2: Guard Clauses

```csharp
public async Task<IActionResult> UpdateProfile(ProfileUpdateDto dto)
{
    return await this.SafeExecuteAsync(
        async () => 
        {
            // Guard clauses - fail fast
            if (dto == null)
                throw new ValidationException("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new[] { "Email is required" } }
                });

            if (dto.Email.Length > 254)
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new[] { "Email is too long" } }
                });

            // Happy path logic
            var profile = await _service.UpdateProfileAsync(dto);
            return Ok(profile);
        },
        _logger
    );
}
```

### Pattern 3: Conditional Exception Throwing

```csharp
public async Task<IActionResult> ApproveApplication(Guid applicationId)
{
    return await this.SafeExecuteAsync(
        async () => 
        {
            var application = await _service.GetApplicationAsync(applicationId)
                ?? throw new ResourceNotFoundException("JobApplication", applicationId);

            // Throw specific exceptions based on state
            if (application.Status != ApplicationStatus.Pending)
                throw new BusinessRuleException(
                    $"Can only approve pending applications. Current status: {application.Status}",
                    errorCode: "INVALID_APPLICATION_STATUS"
                );

            if (application.JobPosting.ClosingDate < DateTime.UtcNow)
                throw new BusinessRuleException(
                    "Cannot approve applications for closed job postings",
                    errorCode: "JOB_CLOSED"
                );

            await _service.ApproveApplicationAsync(applicationId);
            TempData["Success"] = "Application approved successfully";
            
            return RedirectToAction("Details", new { id = applicationId });
        },
        _logger
    );
}
```

## Refactoring Examples

### Before: Manual Error Handling

```csharp
[HttpPost]
public async Task<IActionResult> CreateJobPosting(CreateJobPostingDto dto)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var jobPosting = new JobPosting(dto.Title, dto.Description);
        await _repository.AddAsync(jobPosting);
        await _repository.SaveChangesAsync();

        TempData["Success"] = "Job posting created successfully";
        return RedirectToAction("Index");
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error while creating job posting");
        ModelState.AddModelError(string.Empty, "An error occurred while saving. Please try again.");
        return View(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error");
        ModelState.AddModelError(string.Empty, "An unexpected error occurred");
        return View(dto);
    }
}
```

### After: Centralized Error Handling

```csharp
[HttpPost]
public async Task<IActionResult> CreateJobPosting(CreateJobPostingDto dto)
{
    return await this.SafeExecuteAsync(
        async () => 
        {
            if (!ModelState.IsValid)
                throw new ValidationException(ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ));

            var jobPosting = await _service.CreateJobPostingAsync(dto);
            TempData["Success"] = "Job posting created successfully";
            
            return RedirectToAction("Index");
        },
        _logger
    );
}
```

### Before: Try-Catch in Service

```csharp
public async Task<RegistrationResult> RegisterAsync(RegisterDto dto)
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return new RegistrationResult(false, "Email is required");

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return new RegistrationResult(false, "Email already registered");

        var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return new RegistrationResult(false, result.Errors.First().Description);

        return new RegistrationResult(true, null, applicant: new Applicant { UserId = user.Id });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Registration error");
        return new RegistrationResult(false, "Registration failed");
    }
}
```

### After: Exception-Based Error Handling

```csharp
public async Task<Applicant> RegisterAsync(RegisterDto dto)
{
    // Validate
    var errors = new Dictionary<string, string[]>();
    
    if (string.IsNullOrWhiteSpace(dto.Email))
        errors["Email"] = new[] { "Email is required" };
    
    if (string.IsNullOrWhiteSpace(dto.Password))
        errors["Password"] = new[] { "Password is required" };

    if (errors.Any())
        throw new ValidationException(errors);

    // Check for existing user
    if (await _userManager.FindByEmailAsync(dto.Email) != null)
        throw new BusinessRuleException(
            "Email already registered",
            errorCode: "EMAIL_EXISTS"
        );

    // Create user
    var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
    var result = await _userManager.CreateAsync(user, dto.Password);

    if (!result.Succeeded)
    {
        var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new ApplicationException($"User creation failed: {errorMessage}");
    }

    var applicant = new Applicant { UserId = user.Id, Email = dto.Email };
    await _repository.AddAsync(applicant);
    await _repository.SaveChangesAsync();

    return applicant;
}
```

## Testing Examples

### Unit Test with Exception Throwing

```csharp
[TestFixture]
public class ApplicantServiceTests
{
    private ApplicantService _service;
    private Mock<IApplicantRepository> _mockRepository;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IApplicantRepository>();
        _service = new ApplicantService(_mockRepository.Object, new MockLogger());
    }

    [Test]
    public void GetApplicant_WithInvalidId_ThrowsValidationException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(
            () => _service.GetApplicantAsync(Guid.Empty)
        );
        
        Assert.That(ex.Errors.ContainsKey("id"), Is.True);
    }

    [Test]
    public void GetApplicant_WhenNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetApplicantAsync(id))
            .ReturnsAsync((Applicant)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ResourceNotFoundException>(
            () => _service.GetApplicantAsync(id)
        );
        
        Assert.That(ex.ResourceType, Is.EqualTo("Applicant"));
        Assert.That(ex.ResourceId, Is.EqualTo(id));
    }
}
```

### Integration Test with Controller

```csharp
[TestFixture]
public class ApplicantsControllerTests
{
    private ApplicantsController _controller;
    private Mock<IApplicantService> _mockService;

    [Test]
    public async Task GetApplicant_WhenNotFound_Returns404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetApplicantAsync(id))
            .ThrowsAsync(new ResourceNotFoundException("Applicant", id));

        // Act
        var result = await _controller.GetApplicant(id);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task CreateApplicant_WithValidData_Returns201()
    {
        // Arrange
        var dto = new CreateApplicantDto { Email = "test@test.com" };
        var applicant = new Applicant { Id = Guid.NewGuid(), Email = dto.Email };
        
        _mockService.Setup(s => s.CreateApplicantAsync(dto))
            .ReturnsAsync(applicant);

        // Act
        var result = await _controller.CreateApplicant(dto);

        // Assert
        Assert.IsInstanceOf<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        Assert.That(createdResult.StatusCode, Is.EqualTo(201));
    }
}
```

## Summary

These patterns provide:
- **Clean separation of concerns** - Error handling is centralized
- **Consistent error responses** - All errors follow the same format
- **Better testability** - Exceptions make test assertions clearer
- **Improved maintainability** - Less boilerplate error handling code
- **Professional error handling** - Users see meaningful error messages

Apply these patterns consistently across your controllers and services for robust error handling.

