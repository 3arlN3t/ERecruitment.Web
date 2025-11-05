# Job Posting Closing Date Implementation

## Overview

This document describes the implementation of closing date validation for job postings. The system now prevents applications after a job posting's closing date.

## Changes Made

### 1. **JobPosting Model** (`Models/JobPosting.cs`)

Added two computed properties to the `JobPosting` class:

```csharp
/// <summary>
/// Determines if the job posting is expired based on the closing date.
/// </summary>
/// <returns>True if the closing date has passed, false otherwise.</returns>
public bool IsExpired => ClosingDate.HasValue && ClosingDate.Value < DateTime.UtcNow;

/// <summary>
/// Determines if the job is accepting applications.
/// A job accepts applications if it's active and not expired.
/// </summary>
public bool IsAcceptingApplications => IsActive && !IsExpired;
```

**Purpose:**
- `IsExpired` - Checks if the current date/time is past the closing date
- `IsAcceptingApplications` - Comprehensive check combining both `IsActive` status and expiration

### 2. **ApplicationWorkflowService** (`Services/ApplicationWorkflowService.cs`)

Added closing date validation to four key methods:

#### `StartApplicationAsync`
- Checks if the job has expired before creating a draft application
- Returns error message: `"This position closed on [date]. Applications are no longer accepted."`

#### `SubmitDirectApplicationAsync`
- Validates closing date before submitting direct applications
- Prevents direct submissions for expired positions

#### `SubmitKillerQuestionAsync`
- Validates closing date before accepting screening question answers
- Prevents submission of answers for expired positions

#### `SubmitScreenedApplicationAsync`
- Validates closing date before final submission of screened applications
- Prevents final submission for expired positions

**Validation Logic:**
```csharp
// Check if job has expired
if (job.IsExpired)
{
    return new ApplicationFlowResult(false, 
        $"This position closed on {job.ClosingDate:dd MMM yyyy}. Applications are no longer accepted.");
}

// Check if job is inactive
if (!job.IsActive)
{
    return new ApplicationFlowResult(false, 
        "This position is no longer accepting applications.");
}
```

### 3. **Views/Jobs/Index.cshtml** (Job Listing)

#### Added Variables
```csharp
var isExpired = job.IsExpired;
```

#### Visual Indicators
- **Expired Badge**: Shows a dark "Closed" badge in the top-right corner
- **Styling**: Applies 60% opacity, grey background, and disables pointer events
- **Button State**: Replaces the "View & Apply" button with a disabled "Position Closed" button

#### Implementation Details
```csharp
<div class="job-card fade-in" style="@(hasApplied || isExpired ? "opacity: 0.6; background-color: #f8f9fa; border: 1px solid #dee2e6; pointer-events: none;" : "")">
    @if (hasApplied)
    {
        // ... apply badge logic ...
    }
    else if (isExpired)
    {
        <div class="position-absolute top-0 end-0 m-2">
            <span class="badge bg-dark">
                <i class="fas fa-times-circle me-1"></i>Closed
            </span>
        </div>
    }
    
    // ... later in button section ...
    @if (Model.IsApplicant)
    {
        @if (hasApplied)
        {
            // ... show status ...
        }
        else if (isExpired)
        {
            <button class="btn btn-secondary btn-sm flex-grow-1" disabled title="This position closed on @job.ClosingDate?.ToString("dd MMM yyyy")">
                <i class="fas fa-times-circle me-1"></i>Position Closed
            </button>
        }
        else if (isDraft)
        {
            // ... continue draft ...
        }
        else
        {
            // ... apply button ...
        }
    }
</div>
```

### 4. **Views/Jobs/Details.cshtml** (Job Details Page)

#### Alert Message
Displays a prominent alert when the job is expired:
```csharp
@if (Model.IsExpired)
{
    <div class="alert alert-danger w-100" role="alert">
        <i class="fas fa-exclamation-triangle me-2"></i>
        <strong>Position Closed</strong> - This position closed on <strong>@Model.ClosingDate?.ToString("dd MMM yyyy")</strong>. 
        Applications are no longer being accepted.
    </div>
    <button class="btn btn-secondary" disabled>
        <i class="fas fa-times-circle me-2"></i>Applications Closed
    </button>
}
else if (!Model.IsActive)
{
    <div class="alert alert-warning w-100" role="alert">
        <i class="fas fa-exclamation-circle me-2"></i>
        <strong>Position Unavailable</strong> - This position is no longer accepting applications.
    </div>
    <button class="btn btn-secondary" disabled>
        <i class="fas fa-ban me-2"></i>Not Accepting Applications
    </button>
}
else
{
    <a class="btn btn-primary-gradient" asp-controller="Applications" asp-action="Create" asp-route-jobId="@Model.Id">
        <i class="fas fa-paper-plane me-2"></i>Submit Application
    </a>
}
```

#### Status Sidebar
Enhanced the status display in the Quick Facts sidebar:
```csharp
<div class="fact"><span>Status</span> <strong>
    @if (Model.IsExpired)
    {
        <span class="text-danger">Closed</span>
    }
    else if (!Model.IsActive)
    {
        <span class="text-warning">Inactive</span>
    }
    else
    {
        <span class="text-success">Active</span>
    }
</strong></div>
```

## Behavior

### When a Job Posting Expires

1. **Applicants cannot start new applications** - The system rejects attempts to create new applications
2. **Existing drafts cannot be submitted** - Prevents submitting draft applications after closing
3. **Screening answers cannot be submitted** - Blocks submission of killer question answers
4. **Visual feedback** - The UI shows:
   - "Closed" badge on job cards
   - Disabled buttons with explanatory text
   - Alert message with closing date on details page
   - Color-coded status indicator (red = Closed)

### Error Messages

Users see one of these messages when trying to apply after the closing date:

- **Main Error**: `"This position closed on [date]. Applications are no longer accepted."`
- **Inactive Status**: `"This position is no longer accepting applications."`

## Testing

### Test Case 1: Expired Position (Listing View)
1. Create or modify a job posting with a closing date in the past
2. View the Jobs/Opportunities page
3. **Expected**:
   - Job card shows 60% opacity
   - "Closed" badge appears in top-right corner
   - "Position Closed" button is disabled
   - Pointer events are disabled

### Test Case 2: Expired Position (Details View)
1. Click on an expired job to view details
2. **Expected**:
   - Red alert message displays with closing date
   - "Applications Closed" button is disabled instead of "Submit Application"
   - Status sidebar shows "Closed" in red text

### Test Case 3: Prevent Application Submission
1. Try to submit an application after closing date
2. **Expected**:
   - Backend validation triggers
   - User redirected to dashboard with error message
   - Application is NOT created

### Test Case 4: Active Position (Still Open)
1. View a job with a closing date in the future
2. **Expected**:
   - No "Closed" badge
   - Full opacity (not greyed out)
   - "View & Apply" button is enabled
   - Alert message does not appear

## Technical Details

### DateTime Handling
- All closing date comparisons use `DateTime.UtcNow` for consistency
- The system treats the closing date as end-of-day (UTC)
- If `ClosingDate` is null, the job is considered not to have an expiration

### Validation Layer
- **Frontend**: Visual indicators prevent confused users
- **Backend**: ApplicationWorkflowService enforces business rules
- **Database**: No special database changes needed (uses existing `ClosingDate` field)

### Performance
- Closing date validation is O(1) - simple DateTime comparison
- No database queries added
- Properties are computed on-the-fly (no storage overhead)

## Future Enhancements

Potential improvements:
1. **Auto-close jobs** - Background job to automatically set `IsActive = false` when closing date passes
2. **Reopen functionality** - Allow admins to extend closing dates
3. **Notifications** - Email admins when applications close
4. **Reports** - Closing date analytics and application tracking
5. **Bulk update** - Change closing dates for multiple positions at once
