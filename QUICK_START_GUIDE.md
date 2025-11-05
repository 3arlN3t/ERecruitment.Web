# eRecruitment Portal - Quick Start Guide

## For Applicants

### How to Apply for a Position

**Step 1: Browse Jobs**
1. Log in to your applicant account
2. Click "Browse Jobs" in the main menu OR click "View All Jobs" on your dashboard
3. You'll see all available job postings

**Step 2: View & Apply**
1. Find a job you're interested in
2. Click the **"View & Apply"** button on the job card
3. The application form will open

**Step 3: Complete Application**

**Option A: Direct Submission (No Screening Questions)**
- If the job doesn't require screening questions:
  - Click **"Submit Application"**
  - You'll receive a confirmation email
  - The job will immediately appear greyed out on your dashboard

**Option B: Answer Screening Questions**
- If the job requires screening questions:
  - Read each question carefully
  - Select whether you meet the requirement
  - Click **"Next"** to continue to the next question
  - After all questions are answered:
    - If you passed all: Application is submitted
    - If you failed any: Application is rejected (you'll be notified)

**Step 4: Check Your Dashboard**
1. Go to **"My Dashboard"**
2. View your application statistics:
   - **Total Applications**: All submitted applications
   - **In Review**: Applications awaiting decision
   - **Interview/Offer**: Advanced applications
3. See your "Your Applications" table with all details

**Step 5: Track Application Status**
1. In your applications table, click the **"View Timeline"** icon
2. See the history of your application with timestamps
3. Track any status changes

**Step 6: Withdraw (if needed)**
1. If your application is still in Draft or Submitted status
2. Click the **"Withdraw"** button
3. Confirm the withdrawal
4. The job will become available for reapplication

### Visual Indicators

**Job Status After Applying:**
- Job card becomes lighter/greyed out
- Can't click to reapply
- Badge shows: Applied, In Review, Interview, Offer, or Rejected
- Button changes to "View Application Status"

**Application Status Badges:**
- 🔲 **Draft** (grey): Not yet submitted
- 📧 **Submitted** (blue): Awaiting review
- 👔 **Interview** (green): Interview scheduled
- ✅ **Offer** (green): Offer received
- ❌ **Rejected** (red): Application rejected
- 🚫 **Withdrawn** (grey): You withdrew

---

## For Administrators

### How to View All Applications for a Job Posting

**Step 1: Access Jobs List**
1. Log in with admin credentials
2. Navigate to **Jobs** → **Browse Jobs**
3. You'll see all job postings in a grid format

**Step 2: View Applications for Specific Job**
1. On any job card, click the **blue "Applications" button** (document icon)
2. Alternative: Click **"Edit"** → Scroll down to see applications stats
3. You're now viewing all applications for this specific job

**Step 3: Review Application Statistics**
At the top of the page, you'll see statistics for this job:
- **Total Applications**: All applications received
- **Submitted**: Awaiting review
- **Interview/Offer**: Advanced candidates
- **Rejected**: Rejected applications

**Step 4: Review Detailed Application List**
1. The table shows all applicants with:
   - **Applicant Email**: Who applied
   - **Status**: Current application status (with color-coded badge)
   - **Submitted Date**: When they applied
   - **Outcome/Notes**: Rejection reason or other notes

**Step 5: Pagination (if many applicants)**
- Navigate through pages at the bottom
- Default: 25 applications per page
- Click "First", "Previous", page numbers, "Next", or "Last"

---

### How to View All Applications Across All Jobs

**Step 1: Access Global Applications**
1. From admin menu, click **"Admin" → "All Applications"**
2. Alternative: Direct URL `/AdminApplications`

**Step 2: Filter Applications**
Search and filter options:
1. **Search**: Enter applicant email or job title
2. **Status Filter**: Select specific status (Submitted, Rejected, Interview, Offer, etc.)
3. **Job Filter**: Select specific job posting to filter
4. Click **"Filter"** button to apply

**Step 3: View Results**
- Table shows filtered applications
- Columns: Applicant Email, Job Title, Status, Submitted Date, Outcome
- Results are paginated (25 per page by default)

**Step 4: Export to CSV**
1. Click the **"Export CSV"** button (top right)
2. File downloads as `applications_YYYYMMDDHHMMSS.csv`
3. Open in Excel or your preferred spreadsheet tool
4. CSV includes all currently filtered results

---

## Common Tasks

### Task: Check How Many People Applied for a Job
1. Go to Jobs → Browse Jobs
2. Click "Applications" on the job card
3. Look at the "Total Applications" statistic card

### Task: Find All Rejected Applications
1. Go to Admin → All Applications
2. In Status filter, select "Rejected"
3. Click "Filter"
4. View all rejected applications with reasons

### Task: Check a Specific Applicant's Status
1. Go to Admin → All Applications
2. In search box, type their email address
3. Click "Filter"
4. See all their applications and current statuses

### Task: Export Applications for Reporting
1. Go to Admin → All Applications (or Applications for specific job)
2. Apply any filters you need
3. Click "Export CSV"
4. Open the downloaded file in Excel
5. Create reports as needed

### Task: See Interview Candidates for a Job
1. Go to Jobs → Browse Jobs
2. Click "Applications" on the job
3. Look for applications with "Interview" or "Offer" status
4. These are your advanced candidates

### Task: Update Application Status (Admin)
*Future feature: Admin status change UI coming soon*
- Currently, status is updated through bulk rejection templates
- Direct status updates require database intervention or admin UI enhancement

---

## Important Notes for Applicants

✅ **DO:**
- Keep your profile updated
- Check your email regularly for application updates
- Review the application status timeline
- Withdraw applications you're no longer interested in

❌ **DON'T:**
- Try to submit multiple applications for the same job (system will reject)
- Apply for the same job twice
- Edit application after submission (create new one instead)

---

## Important Notes for Administrators

✅ **DO:**
- Regularly check new applications
- Update applicant status promptly
- Use CSV export for reporting and analysis
- Monitor application statistics per job

❌ **DON'T:**
- Leave applications in Draft status too long
- Forget to send rejection emails
- Lose track of interview candidates

---

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Submit Application | `Ctrl + Enter` on form |
| Go to Dashboard | `Alt + D` |
| Search Applications | `Ctrl + F` (browser search) |

---

## Troubleshooting

### "I can't apply for a job I already applied to"
**Solution:** This is by design. The job is greyed out because you've already applied. 
- If you need to reapply, withdraw your application first
- Then you'll be able to apply again

### "My dashboard doesn't show my application"
**Solution:** 
- Refresh the page (F5)
- Make sure you submitted (not just saved as draft)
- Clear browser cache if still not showing
- Contact admin if still not resolved

### "I got rejected after screening questions"
**Solution:**
- This means you didn't meet a mandatory requirement
- You'll receive an email with details
- Contact admin to appeal if you believe this is in error

### "CSV export is empty"
**Solution:**
- Make sure your filters are correct
- Try exporting without any filters
- Check if there are actually applications matching your criteria

### "Application status won't update"
**Solution:**
- Refresh page (F5)
- If it's been hours, contact admin
- Admin may need to update status manually
- Check email for any status change notifications

---

## Email Notifications

You'll receive automated emails for:

1. **Application Received** ✉️
   - Sent immediately after you submit
   - Confirmation your application was received

2. **Application Update** 📬
   - Sent when status changes (Interview, Offer, Rejected)
   - Includes relevant details and next steps

3. **Application Withdrawn** 🚫
   - Sent when you withdraw an application

**Tip:** Check your spam/junk folder if you don't see emails!

---

## Technical Details

**Database:**
- All applications stored with timestamps (UTC)
- Automatic audit trail for compliance
- No manual database edits required

**File Size Limits:**
- CV: [Set in application config]
- Other Documents: [Set in application config]

**Browser Compatibility:**
- Chrome 90+
- Firefox 88+
- Edge 90+
- Safari 14+

---

## Getting Help

**For Applicants:**
- Email: [Support Email]
- FAQ: [FAQ Page]
- Contact: [Contact Form on website]

**For Administrators:**
- Technical Issues: [IT Support]
- Data Questions: [Admin Support]
- Report a Bug: [Bug Report Form]

---

## Version Information

- **System Version:** 1.0.0
- **Last Updated:** 2024
- **Database:** EF Core with [Your DB]
- **Framework:** ASP.NET Core 8.0+

---

## Success Scenarios

### Scenario 1: New Applicant Applying for First Time
1. Applicant registers account
2. Completes profile with CV
3. Browses jobs
4. Clicks "View & Apply" on interesting position
5. Completes screening questions
6. Receives confirmation email
7. Checks dashboard - sees application status "Submitted"
8. Job card is now greyed out
9. Admin reviews and advances to interview
10. Applicant receives "Interview" status notification

### Scenario 2: Admin Reviewing Applications
1. Admin logs in
2. Navigates to Jobs → Browse Jobs
3. Clicks "Applications" on high-volume position
4. Sees 45 total applications
5. 38 submitted, 5 passed screening, 2 moved to interview
6. Reviews candidates in interview stage
7. Exports list to CSV for hiring meeting
8. Updates statuses based on interview outcomes
9. Sends offer to selected candidate

---

## Related Documentation

- See `APPLICATION_FEATURES_GUIDE.md` for technical architecture
- See `IMPLEMENTATION_NOTES.md` for system setup
- See `MODEL_VERIFICATION_REPORT.md` for data model details

---

Good luck! 🚀
