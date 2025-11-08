# Data Saving Issue - Diagnostic Guide

**Date:** 2025-11-08  
**Status:** 🔧 Enhanced with better logging and error handling

---

## Problem

Data entered by users is not being saved to the database, even though no errors are reported.

---

## Enhancements Made

### 1. **Improved `UpdateJobPostingAsync`**
   - Now uses the same pattern as `UpdateApplicantAsync`
   - Properly handles both tracked and detached entities
   - Calls `DetectChanges()` for tracked entities
   - Added logging to track save operations

### 2. **Enhanced Logging**
   - Both `UpdateApplicantAsync` and `UpdateJobPostingAsync` now log:
     - Number of changes saved
     - Warnings when no changes are detected
     - Errors when save operations fail

### 3. **Better Error Handling**
   - Exceptions are now logged with full context
   - Entity state is logged to help diagnose issues

---

## How to Diagnose the Issue

### Step 1: Check Application Logs

When you try to save data, check the application logs for:

```bash
# If running in foreground
# Look for log messages like:
# - "Updated Applicant {ApplicantId}. Changes saved: {ChangesCount}"
# - "Updated JobPosting {JobId}. Changes saved: {ChangesCount}"
# - "No changes were saved for..."
# - "Failed to save..."

# If running in background
tail -f /tmp/app_running.log | grep -E "(Updated|Failed|changes)"
```

### Step 2: Verify Database Connection

```bash
# Check if SQL Server is running
docker ps | grep erecruitment-sqlserver

# Test database connection
docker exec erecruitment-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '!@Otsile12' \
  -Q "SELECT COUNT(*) FROM Applicants"
```

### Step 3: Check for Exceptions

Look for any exceptions in the logs that might be swallowed:

```bash
# Search for exceptions
grep -i "exception\|error\|fail" /tmp/app_running.log | tail -20
```

### Step 4: Verify Data in Database

Connect to the database and check if data exists:

```bash
# Using docker exec
docker exec -it erecruitment-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '!@Otsile12' \
  -d ERecruitment \
  -Q "SELECT TOP 10 Id, Email, Profile_FirstName, Profile_LastName FROM Applicants ORDER BY CreatedAtUtc DESC"
```

### Step 5: Test Save Operation

1. **Enable Detailed Logging:**
   - The application already has `EnableSensitiveDataLogging()` enabled in Development mode
   - Check `appsettings.json` to ensure logging level is set correctly

2. **Try Saving Data:**
   - Update a profile field
   - Check logs immediately after
   - Look for the "Updated Applicant" message

---

## Common Issues and Solutions

### Issue 1: "No changes were saved" Warning

**Cause:** EF Core didn't detect any changes to save.

**Possible Reasons:**
- Entity properties weren't actually modified (same values)
- Entity is detached and `Update()` didn't work properly
- Change tracker isn't detecting owned entity changes

**Solution:**
- Check the log for entity state
- Verify that you're actually modifying the entity properties
- Ensure the entity is properly attached to the DbContext

### Issue 2: Exception During Save

**Cause:** Database error or constraint violation.

**Solution:**
- Check the exception message in logs
- Verify database constraints
- Check for foreign key violations
- Ensure all required fields are populated

### Issue 3: Data Saved But Not Visible

**Cause:** 
- Reading from a different database/context
- Caching issues
- Transaction not committed

**Solution:**
- Verify you're reading from the same database
- Clear browser cache
- Check if there are multiple DbContext instances

---

## Code Changes Summary

### `UpdateApplicantAsync` (Enhanced)
```csharp
public async Task UpdateApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default)
{
    var entry = _db.Entry(applicant);
    
    if (entry.State == EntityState.Detached)
    {
        _db.Applicants.Update(applicant);
    }
    else
    {
        _db.ChangeTracker.DetectChanges(); // Ensures owned entity changes are detected
    }

    try
    {
        var changesSaved = await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Updated Applicant {ApplicantId}. Changes saved: {ChangesCount}", applicant.Id, changesSaved);
        
        if (changesSaved == 0)
        {
            _logger.LogWarning("No changes were saved for Applicant {ApplicantId}. Entity state: {State}", applicant.Id, entry.State);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to save Applicant {ApplicantId}. Entity state: {State}", applicant.Id, entry.State);
        throw;
    }
}
```

### `UpdateJobPostingAsync` (Fixed)
```csharp
public async Task UpdateJobPostingAsync(JobPosting job, CancellationToken cancellationToken = default)
{
    var entry = _db.Entry(job);
    
    if (entry.State == EntityState.Detached)
    {
        _db.JobPostings.Update(job);
    }
    else
    {
        _db.ChangeTracker.DetectChanges(); // Ensures all changes are detected
    }

    try
    {
        var changesSaved = await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Updated JobPosting {JobId}. Changes saved: {ChangesCount}", job.Id, changesSaved);
        
        if (changesSaved == 0)
        {
            _logger.LogWarning("No changes were saved for JobPosting {JobId}. Entity state: {State}", job.Id, entry.State);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to save JobPosting {JobId}. Entity state: {State}", job.Id, entry.State);
        throw;
    }
}
```

---

## Next Steps

1. **Restart the application** to apply the changes:
   ```bash
   pkill -f "dotnet run"
   cd /home/ole/Documents/ERecruitment.Web
   dotnet run
   ```

2. **Try saving data** and watch the logs

3. **Check the logs** for:
   - "Updated Applicant/JobPosting" messages
   - "No changes were saved" warnings
   - Any exceptions

4. **Report findings:**
   - What operation were you trying to perform?
   - What do the logs show?
   - Are there any exceptions?

---

## Additional Debugging

If issues persist, enable more detailed EF Core logging by checking `Program.cs`:

```csharp
if (builder.Environment.IsDevelopment())
{
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    options.LogTo(Console.WriteLine, LogLevel.Information); // Already enabled
}
```

This will show all SQL queries being executed, which can help identify if `SaveChangesAsync` is actually being called and what SQL is being generated.

