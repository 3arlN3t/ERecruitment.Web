# eRecruitment Portal - Application Features Guide

## Overview

This document details the complete applicant application management system in the eRecruitment Portal, including how applicants apply for positions, how their dashboard is updated, and how administrators can view applications per job posting.

---

## 1. Applicant Application Workflow

### 1.1 Application Submission Process

**Flow:**
1. Applicant browses available job postings via `Jobs/Index`
2. Applicant clicks "View & Apply" on a job posting
3. System creates a draft application via `ApplicationsController.Job()`
4. Applicant either:
   - **Direct Submission**: Submits directly if job has no screening questions
   - **Screening Questions**: Answers killer questions before submission

**Key Files:**
- Controller: `ApplicationsController.cs`
  - `Job()` - View application details
  - `SubmitDirectApplication()` - Submit without questions
  - `KillerQuestion()` - Handle screening questions
- Service: `ApplicationService.cs`
  - `StartApplication()` - Create draft application
  - `SubmitDirectApplication()` - Submit direct applications
  - `SubmitKillerQuestion()` - Process screening answers

### 1.2 Application Status Lifecycle

```
Draft → Submitted → Review Process → Interview/Offer
                                   → Rejected
                                   → Withdrawn
```

**Status States:**
- **Draft**: Application in progress, not yet submitted
- **Submitted**: Application submitted for review
- **Rejected**: Application rejected (includes auto-rejection for failed screening)
- **Interview**: Applicant invited to interview
- **Offer**: Job offer extended
- **Withdrawn**: Applicant withdrew application

**Implementation:** `ApplicationStatus` enum in `Models/ApplicationStatus.cs`

---

## 2. Preventing Duplicate Applications & Greying Out Posts

### 2.1 Applied Jobs Display

When an applicant has submitted an application, the job posting is **greyed out** and marked as unavailable for reapplication.

**Visual Indicators:**
- Applied jobs show reduced opacity (0.6)
- Grey background with muted styling
- Status badge displays (Applied, In Review, Interview, Offer, Rejected, Withdrawn)
- Button changes to "View Application Status" instead of "View & Apply"

**Implementation Locations:**

1. **Dashboard (Applicant/Dashboard.cshtml)**
   - Line 192-274: Displays open positions with application status
   - Applied jobs (hasApplied = true) show reduced opacity and grey background
   - Buttons change based on application status

2. **Jobs Listing (Jobs/Index.cshtml)**
   - Line 73-182: Shows all available jobs
   - For authenticated applicants:
     - Checks `existingApplication` from `ApplicantApplications`
     - Marks `hasApplied = existingApplication != null && existingApplication.Status != ApplicationStatus.Draft`
     - Applies opacity: 0.6 and grey styling
     - Disables interaction with `pointer-events: none`

### 2.2 Duplicate Prevention Logic

**Implemented in `ApplicationService.SubmitDirectApplication()`:**
```csharp
// Check if already submitted
if (application.Status == ApplicationStatus.Submitted)
{
    return new ApplicationFlowResult(false, "You have already submitted an application for this position.");
}
```

**Database Constraint:**
- `Applicant.Applications` is a List<JobApplication>
- Unique constraint: Each applicant can only have one application per job posting
- Enforced by the application logic checking `FirstOrDefault(a => a.JobPostingId == jobId)`

---

## 3. Applicant Dashboard - Application Status Updates

### 3.1 Dashboard Overview

**Location:** `Views/Applicant/Dashboard.cshtml`

**Statistics Cards Display (Lines 24-62):**
```
┌─────────────────────────────────────────────────────┐
│ Total Applications │ In Review │ Interview/Offer │   │
│      (Count)       │  (Count)  │    (Count)      │   │
└─────────────────────────────────────────────────────┘
```

**Calculated Values:**
- **Total Applications**: `Model.Applications.Count(a => a.Status != ApplicationStatus.Draft)`
- **In Review**: `Model.Applications.Count(a => a.Status == ApplicationStatus.Submitted)`
- **Interview/Offer**: `Model.Applications.Count(a => a.Status is ApplicationStatus.Interview or ApplicationStatus.Offer)`
- **Open Positions**: `Model.Jobs.Count`

### 3.2 Your Applications Table

**Features:**
- Lists all submitted and active applications
- Shows application status with color-coded badges:
  - Draft: Grey badge
  - Submitted: Blue badge
  - Interview: Green badge
  - Offer: Green badge
  - Rejected: Red badge
  - Withdrawn: Grey badge
- Last update timestamp
- Notes about rejection reasons or failed requirements
- Action buttons:
  - View Timeline: See application history
  - Withdraw: Withdraw active application

**Implementation:**
- Populated via `ApplicantController.Dashboard()`:
  ```csharp
  Applications = _service.GetApplications(applicant)
  ```
- `ApplicationService.GetApplications()` returns all applications for the applicant

### 3.3 Open Positions Section

**Context-Aware Display:**
- Shows all available jobs on the dashboard
- Each job card reflects applicant's application status
- Allows quick access to browse or continue draft applications

---

## 4. Administrator Features - View Applications Per Job Post

### 4.1 Admin Views All Applications Per Job Posting

**Primary View:** `Views/Jobs/Applications.cshtml`

**Access Point:**
- Admin clicks "Applications" button on job card in `Jobs/Index`
- Route: `GET /Jobs/Applications/{jobId}`
- Controlled by: `JobsController.Applications()`

### 4.2 Applications Per Job Statistics

**Statistics Panel (Lines 29-70):**
```
┌─────────────────────────────────────────────────────┐
│ Total │ Submitted │ Interview/Offer │ Rejected      │
└─────────────────────────────────────────────────────┘
```

**Metrics Displayed:**
- Total Applications for this job
- Submitted: Awaiting review
- Interview/Offer: Advanced candidates
- Rejected: Rejected applications

**Implementation:**
```csharp
@Model.Applications.Count(a => a.Status == ApplicationStatus.Submitted)
@Model.Applications.Count(a => a.Status == ApplicationStatus.Interview || 
                           a.Status == ApplicationStatus.Offer)
@Model.Applications.Count(a => a.Status == ApplicationStatus.Rejected)
```

### 4.3 Detailed Application List

**Table Display (Lines 73-150):**
- Applicant Email
- Application Status (with badges)
- Submitted Date
- Outcome/Rejection Reason Notes

**Pagination:**
- Default: 25 items per page
- Configurable via `pageSize` parameter
- Navigation controls for easy browsing

**Implementation:**
```csharp
var paged = _repo.GetJobApplications(page, pageSize, jobId: id);
```

---

## 5. Global Admin Applications View

### 5.1 All Applications Across All Jobs

**View:** `Views/AdminApplications/Index.cshtml`

**Features:**
- See ALL submitted applications across ALL job postings
- Filter by:
  - Email/Job Title (search)
  - Application Status
  - Job Posting
- Export to CSV for reporting

**Access:** `AdminApplicationsController.Index()`

### 5.2 Admin Filtering & Export

**Filter Options:**
- Search by applicant email or job title
- Filter by application status (Submitted, Rejected, Interview, Offer, etc.)
- Filter by specific job posting
- Pagination for large datasets

**Export Function:**
- `ExportCsv()` generates downloadable CSV
- Format: ApplicationId, ApplicantEmail, JobTitle, Status, SubmittedAt, Outcome
- Includes all filtered results

---

## 6. Technical Architecture

### 6.1 Data Models

**JobApplication.cs:**
```csharp
public class JobApplication
{
    public Guid Id { get; set; }
    public Guid ApplicantId { get; set; }
    public Guid JobPostingId { get; set; }
    public required string JobTitle { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public List<ScreeningAnswer> ScreeningAnswers { get; set; }
    public string? RejectionReason { get; set; }
    public List<AuditEntry> AuditTrail { get; set; }
}
```

**Applicant.cs:**
```csharp
public class Applicant
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public List<JobApplication> Applications { get; set; } = new();
    // ... other properties
}
```

### 6.2 Service Layer

**IApplicationService** provides:
- `StartApplication()` - Create draft
- `SubmitDirectApplication()` - Submit without screening
- `SubmitKillerQuestion()` - Handle screening questions
- `WithdrawApplication()` - Withdrawal handling
- `GetApplications()` - Retrieve applicant's applications
- `GetJobPosting()` - Get job details

**IRecruitmentRepository** provides:
- `GetJobApplications()` - Paginated list with filtering
- `GetJobPostings()` - All available jobs
- `GetApplicants()` - All applicants

### 6.3 View Models

**BrowseJobsViewModel:**
- Jobs: All available job postings
- ApplicantApplications: Current applicant's applications
- IsApplicant, IsAdmin: Role flags

**ApplicantDashboardViewModel:**
- Applicant: Current applicant details
- Jobs: Available job postings
- Applications: Applicant's applications

**JobApplicationsViewModel:**
- JobPosting: Current job details
- Applications: Paginated applications for job
- Page, PageSize, TotalCount: Pagination info

---

## 7. User Experience Flow

### 7.1 Applicant Journey

```
1. Browse Jobs
   ↓
2. Click "View & Apply"
   ↓
3. Application Starts (Draft Created)
   ↓
4. Answer Screening Questions (if required)
   ↓
5. Submit Application
   ↓
6. View Updated Dashboard
   ↓
7. Job Post Appears Greyed Out
   ↓
8. Track Application Status
```

### 7.2 Admin Journey

```
1. View Jobs List
   ↓
2. Click "Applications" on Specific Job
   ↓
3. See All Applications for That Job
   ↓
4. View Statistics & Detailed List
   ↓
5. (Optional) Export to CSV
   ↓
6. (Optional) View All Applications Globally
```

---

## 8. Styling & Visual Design

### 8.1 Application Status Badges

| Status | Color | Icon |
|--------|-------|------|
| Draft | Grey | pencil-alt |
| Submitted | Blue | paper-plane |
| Interview | Green | user-tie |
| Offer | Green | check-circle |
| Rejected | Red | times-circle |
| Withdrawn | Grey | ban |

### 8.2 Job Card States

**Available Job:**
- Full opacity, clickable
- Button: "View & Apply"

**Applied Job:**
- Opacity: 0.6
- Background: #f8f9fa
- Border: 1px solid #dee2e6
- Status badge displayed
- Button: "View Application Status"

**Implementation:** `site.css` uses:
```css
opacity: 0.6;
background-color: #f8f9fa;
border: 1px solid #dee2e6;
pointer-events: none;
```

---

## 9. Audit Trail & Tracking

Every application action is logged with:
- Actor (applicant email or "system")
- Action description
- Timestamp (UTC)

**Example Actions:**
- "Submitted application via direct submission"
- "Submitted application after passing killer question"
- "Application auto-rejected after failing killer question"
- "Application withdrawn: User requested"

---

## 10. Email Notifications

Automated emails sent for:
- Application Received: Confirmation after submission
- Application Rejected: With personalized reason
- Application Withdrawn: Confirmation of withdrawal
- Status Updates: When admin changes application status

Email templates rendered via `IEmailTemplateRenderer`

---

## 11. Configuration & Customization

### 11.1 Configurable Settings

- **Pagination Size:** Default 25 items per page
- **Screening Questions:** Per-job, up to unlimited questions
- **Application Statuses:** Enum-based, easily extensible
- **Email Templates:** Separate template files for customization

### 11.2 Adding New Application Statuses

To add a new status (e.g., "Shortlisted"):

1. Add to `ApplicationStatus` enum:
   ```csharp
   public enum ApplicationStatus
   {
       Draft,
       Submitted,
       Shortlisted,  // New
       // ...
   }
   ```

2. Update dashboard statistics as needed

3. Add UI badge styling in Razor views

---

## 12. Security Considerations

- **Authentication:** Applicants must be logged in to apply
- **Authorization:** Admins can only access admin views via `[Authorize(Roles = "Admin")]`
- **Isolation:** Applicants only see their own applications
- **Data Validation:** Anti-forgery tokens on all POST requests
- **Audit Trail:** All actions logged for compliance

---

## 13. Testing Recommendations

### 13.1 Applicant Features

- [ ] Apply for job without screening questions
- [ ] Apply for job with screening questions
- [ ] Verify job card greyed out after application
- [ ] Withdraw application and verify reapply option appears
- [ ] Verify dashboard statistics update correctly
- [ ] Test email notifications

### 13.2 Admin Features

- [ ] View applications for specific job posting
- [ ] Verify statistics calculate correctly
- [ ] Test filtering and search
- [ ] Export to CSV functionality
- [ ] Verify pagination works with multiple pages
- [ ] Test global applications view

---

## 14. Key Features Summary

✅ **Applicants can apply for positions**
- Direct submission for jobs without screening questions
- Screening question process for jobs with mandatory questions
- Draft save functionality

✅ **Posts greyed out after application**
- Reduced opacity (60%)
- Grey background styling
- Status badges indicating application state
- Disabled re-application to same job

✅ **Dashboard updated with application status**
- Total applications count
- In Review count
- Interview/Offer count
- Individual application details with status
- Last update timestamps
- Rejection reason notes

✅ **Admin can see all applications per job post**
- Dedicated view per job posting
- Statistics card showing status breakdown
- Detailed table with applicant information
- Pagination for large datasets
- Export to CSV functionality
- Global view of all applications across jobs

---

## 15. Future Enhancement Ideas

- Application scoring/ranking system
- Bulk status updates for admins
- Application pipeline visualization
- Advanced search with date ranges
- Email templates customization UI
- Integration with external ATS
- Application analytics dashboard
- Scheduling interview dates
- Video resume support
