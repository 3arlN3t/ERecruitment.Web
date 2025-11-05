# Closed Job Posting Visibility & Reopen Implementation

## Overview

This document describes the implementation of role-based visibility for closed/expired job postings and the ability for admins to reopen positions with updated closing dates.

## Key Changes

### 1. **Job Visibility Filtering** (`Controllers/JobsController.cs`)

#### Index Action - Role-Based Filtering
```csharp
// Filter jobs: applicants only see open jobs, admins see all
var jobs = isApplicant 
    ? allJobs.Where(j => j.IsAcceptingApplications).ToList()
    : allJobs;
```

**Behavior:**
- **Applicants**: See only jobs where `IsAcceptingApplications == true` (active AND not expired)
- **Admins**: See ALL job postings (open and closed)
- **Anonymous Users**: See only open jobs

#### Details Action - Applicant Restriction
```csharp
// Prevent applicants from viewing closed/expired jobs
if (isApplicant && !job.IsAcceptingApplications)
{
    return RedirectToAction(nameof(Index));
}
```

**Behavior:**
- **Applicants**: Attempting to access closed jobs redirects to listing page
- **Admins**: Can always view job details, even if closed

### 2. **Reopen/Republish Functionality** (`Controllers/JobsController.cs`)

#### New Action: ReopenModal (GET)
```csharp
[HttpGet]
public async Task<IActionResult> ReopenModal(Guid id)
{
    var job = await _repo.GetJobPostingAsync(id);
    var viewModel = new ReopenJobViewModel
    {
        Id = job.Id,
        Title = job.Title,
        CurrentClosingDate = job.ClosingDate,
        NewClosingDate = DateTime.UtcNow.AddDays(30) // Smart default: 30 days
    };
    return PartialView("_ReopenJobModal", viewModel);
}
```

**Features:**
- Loads modal via AJAX (no page reload)
- Shows current closing date
- Suggests default 30 days in the future
- Admin can customize the new date

#### New Action: Reopen (POST)
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Reopen(Guid id, DateTime newClosingDate)
{
    // Validation: new date must be in the future
    if (newClosingDate <= DateTime.UtcNow)
    {
        TempData["Flash"] = "New closing date must be in the future.";
        return RedirectToAction(nameof(Index));
    }

    // Reopen the job
    job.IsActive = true;
    job.ClosingDate = newClosingDate;
    job.DateLastModified = DateTime.UtcNow;

    await _repo.UpdateJobPostingAsync(job);
    TempData["Flash"] = $"Position '{job.Title}' has been reopened and will close on {newClosingDate:dd MMM yyyy}.";
    return RedirectToAction(nameof(Index));
}
```

**Features:**
- Sets `IsActive = true`
- Updates closing date to admin-selected value
- Validates that new date is in the future
- Shows success confirmation message
- Updates `DateLastModified` timestamp

### 3. **ReopenJobViewModel** (`ViewModels/ReopenJobViewModel.cs`)

```csharp
public class ReopenJobViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? CurrentClosingDate { get; set; }
    public DateTime NewClosingDate { get; set; }
}
```

### 4. **Reopen Modal Partial View** (`Views/Jobs/_ReopenJobModal.cshtml`)

**Features:**
- Bootstrap modal with form
- Shows position title
- Displays previous closing date
- Date input field with:
  - Suggested date (30 days from now)
  - Minimum date validation (future only)
- Info alert explaining what happens when reopened
- Confirm/Cancel buttons

### 5. **Job Listing Updates** (`Views/Jobs/Index.cshtml`)

#### Closed Job Indicators (Admin Only)
```csharp
@if (isExpired)
{
    <div class="position-absolute top-0 end-0 m-2">
        <span class="badge bg-dark">
            <i class="fas fa-times-circle me-1"></i>Closed
        </span>
    </div>
}
```

#### Reopen Button (Admin Only)
```csharp
@if (isExpired)
{
    <button type="button" class="btn btn-outline-success btn-sm" 
            title="Reopen" 
            data-bs-toggle="modal" 
            data-bs-target="#reopenJobModal" 
            onclick="loadReopenModal('@job.Id')">
        <i class="fas fa-sync-alt"></i>
    </button>
}
```

#### AJAX Modal Loading Script
```javascript
function loadReopenModal(jobId) {
    fetch(`/Jobs/ReopenModal/${jobId}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById('modalContainer').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('reopenJobModal'));
            modal.show();
        })
        .catch(error => console.error('Error loading modal:', error));
}
```

### 6. **Job Applications View Updates** (`Views/Jobs/Applications.cshtml`)

#### Closed Job Badge
```csharp
@if (Model.JobPosting.IsExpired)
{
    <span class="badge bg-danger ms-2">
        <i class="fas fa-times-circle me-1"></i>Closed
    </span>
}
```

#### Archived Applications Notice
```csharp
@if (Model.JobPosting.IsExpired)
{
    <div class="alert alert-info mt-3 mb-0">
        <i class="fas fa-info-circle me-2"></i>
        This position is closed. These applications are archived.
        @if (!Model.JobPosting.IsActive)
        {
            <a href="javascript:loadReopenModal('@Model.JobPosting.Id')" class="alert-link">
                Click here to reopen this position.
            </a>
        }
    </div>
}
```

## User Experience by Role

### **For Applicants:**
1. ✅ Browse job listings - only see open positions
2. ✅ Cannot find closed jobs in search/listing
3. ✅ If they try to access closed job URL directly - redirected to listing
4. ✅ Cannot see "Position Closed" or application history for closed jobs

### **For Admins:**
1. ✅ See ALL jobs in listing (open and closed)
2. ✅ Closed jobs show "Closed" badge
3. ✅ Can view job details (even if closed)
4. ✅ Can view applications for any job
5. ✅ Can see "Reopen" button on closed jobs
6. ✅ Click Reopen → Modal opens with date picker
7. ✅ Set new closing date → Confirm → Job becomes active again
8. ✅ Applications section shows "Archived" notice
9. ✅ Previous applications remain accessible

## Workflow: Closing and Reopening a Position

### Initial Creation
```
Admin creates job → Set closing date → Job is Active & Open
```

### Job Expires Naturally
```
Closing date passes → Job becomes Expired
├─ Applicants: Job disappears from listing
└─ Admins: Job shows with "Closed" badge
```

### Admin Reopens Position
```
Admin clicks Reopen button
├─ Modal opens
├─ Shows previous closing date
├─ Admin selects new date (30-day default)
└─ Admin confirms
    ├─ IsActive = true
    ├─ ClosingDate = new date
    ├─ Job becomes visible to applicants
    └─ Applications section shows active status
```

## Database Considerations

**No migration needed** - Uses existing fields:
- `IsActive` - Controls whether position is accepting applications
- `ClosingDate` - DateTime field for expiration
- `DateLastModified` - Updated when reopened

## Testing Scenarios

### Test 1: Applicant Cannot See Closed Jobs
```
1. Create job with closing date = yesterday
2. Login as Applicant
3. Go to Jobs/Index
4. ✓ Job should NOT appear in list
5. Try to access job Details URL directly
6. ✓ Should redirect to Index
```

### Test 2: Admin Can See & Reopen Closed Jobs
```
1. Create job with closing date = yesterday
2. Login as Admin
3. Go to Jobs/Index
4. ✓ Job appears with "Closed" badge
5. Click "Reopen" button
6. ✓ Modal opens with date picker
7. Select future date (e.g., 30 days from now)
8. ✓ Click Confirm
9. ✓ Job becomes Active again
10. Logout → Login as Applicant
11. ✓ Job now visible in listing
```

### Test 3: Applications Remain Accessible
```
1. Close a job that has applications
2. Login as Admin
3. Go to Job Applications page
4. ✓ See "Archived" notice
5. ✓ All applications still visible
6. Reopen the job
7. ✓ "Archived" notice removed
8. ✓ Applications still accessible
```

## Security Considerations

1. **Authorization**: Only Admins can access reopen functionality
2. **Validation**: New closing date must be in the future
3. **Audit Trail**: `DateLastModified` tracks when jobs are reopened
4. **Data Protection**: Previous applications remain intact and visible to admins

## Files Modified

```
✅ Controllers/JobsController.cs              - Added Index filtering, ReopenModal, Reopen actions
✅ Views/Jobs/Index.cshtml                   - Added "Closed" badge, Reopen button, AJAX script
✅ Views/Jobs/Applications.cshtml            - Added closed indicator and archived notice
✅ ViewModels/ReopenJobViewModel.cs          - New ViewModel for reopen modal
✅ Views/Jobs/_ReopenJobModal.cshtml         - New partial view for reopen modal
```

## Future Enhancements

1. **Bulk Reopen** - Reopen multiple positions at once
2. **Auto-Archive** - Automatically hide closed jobs from listing after 30 days
3. **Notifications** - Email admins when positions are about to close
4. **Audit Log** - Track who reopened positions and when
5. **Extension Option** - Extend closing date without fully reopening
