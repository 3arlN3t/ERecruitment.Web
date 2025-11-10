# Applied Jobs Separation Implementation

## Overview
Implemented visual feedback for applied jobs on the Applicant Dashboard. When an applicant applies for a position, the job card is greyed out with an "Applied" badge, preventing duplicate applications while keeping the job visible in the "Open Positions" section for reference.

## Changes Made

### 1. View: Dashboard.cshtml
**Location:** `Views/Applicant/Dashboard.cshtml` lines 278-364

**Changes:**
- Restored greyed-out styling for applied jobs (opacity-75 and background color)
- Show "Applied" status badge on job cards for applied positions
- Display "View Application Status" button instead of "View & Apply" for applied jobs
- Display "View Timeline" button for jobs in Submitted, Interview, or Offer status
- Keep draft applications showing "Continue Draft" button

## Behavior

### Applied Job Display
- Applied jobs remain visible in "Open Positions" section
- Job cards are greyed out (opacity-75 with light grey background)
- Status badge shows: "Applied", "Interview", "Offer", or "Rejected" depending on application status
- "View & Apply" button is replaced with "View Application Status" and timeline buttons
- Prevents accidental duplicate applications (primary action is to view status, not apply)

### Draft Applications
- Jobs with draft applications show "Continue Draft" button
- Can be completed to submit the application

### Unapplied Jobs
- Display normally with "View & Apply" button
- Can be clicked to start an application

## Business Rules

1. **Applied Status Check** = If an application exists AND status is NOT Draft
2. **Visual Indicator** = Greyed-out card + status badge on applied jobs
3. **Button Logic**:
   - Applied (non-draft) → "View Application Status" button
   - Draft → "Continue Draft" button
   - Unapplied → "View & Apply" button
4. **Timeline Button** → Only shown for Submitted, Interview, or Offer status

## Testing Recommendations

1. **Test Case 1: New Applicant**
   - Applicant with no applications should see all open jobs in normal state
   - All jobs should have "View & Apply" button
   - "Your Applications" section should be empty

2. **Test Case 2: Applied for Job**
   - After applying, the job should appear greyed out in "Open Positions"
   - "Applied" badge should display in top-right corner
   - "View Application Status" button should be visible
   - Job should also appear in "Your Applications" with "Submitted" status

3. **Test Case 3: Draft Application**
   - A job with a draft application should appear normal (not greyed)
   - Button should show "Continue Draft"
   - Should not appear in "Your Applications" (only active applications)

4. **Test Case 4: Interview Status**
   - Job should appear greyed out with "Interview" badge
   - Both "View Application Status" and "View Timeline" buttons visible

5. **Test Case 5: Offer Status**
   - Job should appear greyed out with "Offer" badge
   - Both "View Application Status" and "View Timeline" buttons visible

6. **Test Case 6: Rejected Application**
   - Job should appear greyed out with "Rejected" badge
   - "View Application Status" button visible
   - No "View Timeline" button

## Implementation Details

### Visual Styling
```html
<!-- For applied jobs (hasApplied = true) -->
<div class="job-card opacity-75" style="background-color: #f8f9fa; border: 1px solid #dee2e6;">
    <div class="position-absolute top-0 end-0 m-2">
        <span class="badge bg-info">Applied</span>
    </div>
    ...
</div>

<!-- For unapplied jobs (hasApplied = false) -->
<div class="job-card">
    ...
</div>
```

## No Breaking Changes
- No database schema changes
- No model changes
- No API changes
- Original functionality preserved
- Build successful with zero errors/warnings

## Benefits
- ✅ Clear visual indicator for applied jobs
- ✅ Prevents accidental duplicate applications
- ✅ Jobs remain visible for reference
- ✅ Easy access to application status
- ✅ Clear action buttons for each state
- ✅ Familiar UI pattern (matches original design intent)
