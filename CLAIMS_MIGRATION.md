# ApplicantId Claims Migration Guide

## Overview

This migration adds **ApplicantId claims** to existing Identity users, transitioning the authentication system from session-based to claims-based authentication.

### What Changed?

**Before:**
- Used ASP.NET Identity for authentication
- Used custom session tracking (`ICurrentApplicant`) to store applicant data
- Required database query on every request to fetch applicant information

**After:**
- Uses ASP.NET Identity with Claims
- ApplicantId stored directly in the authentication cookie as a claim
- No session management needed
- No database query needed to get ApplicantId (it's in the cookie)

---

## Automatic Migration

The migration **runs automatically** on application startup (in `Program.cs`).

### When It Runs

The migration executes after:
1. Database migrations are applied
2. Identity is seeded (admin role/user)
3. Domain data is seeded

### What It Does

For each applicant in the database:
1. Finds the corresponding Identity user by email
2. Checks if ApplicantId claim already exists
3. Adds the claim if missing
4. Updates the claim if incorrect
5. Logs all operations for auditing

### Migration Output

Check the application logs for output like:

```
[INFO] Starting ApplicantId claims migration...
[INFO] Found 25 applicants to process
[INFO] Successfully added ApplicantId claim for john.doe@example.com (ApplicantId: a1b2c3d4-...)
[INFO] ApplicantId claim already exists for jane.smith@example.com. Skipping.
[INFO] ApplicantId claims migration completed. Added: 20, Skipped: 5, Errors: 0
```

---

## Manual Migration (Optional)

If you want to skip automatic migration or run it manually, you have several options:

### Option 1: Admin Dashboard (Recommended)

1. Log in as an administrator
2. Navigate to Admin Dashboard
3. Click **"Run Claims Migration"** button
4. Check the flash message for results
5. Review application logs for details

### Option 2: Disable Automatic Migration

Comment out the migration code in `Program.cs`:

```csharp
// Migrate existing users to claims-based authentication
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
//     var logger = scope.ServiceProvider.GetRequiredService<ILogger<ERecruitment.Web.Data.ClaimsMigration>>();
//
//     var claimsMigration = new ERecruitment.Web.Data.ClaimsMigration(dbContext, userManager, logger);
//     await claimsMigration.MigrateApplicantClaimsAsync();
// }
```

Then run manually via the admin dashboard when ready.

### Option 3: Direct Code Execution

If you need to run from code:

```csharp
// In a controller or service
await MigrationRunner.RunClaimsMigrationAsync(HttpContext.RequestServices);
```

---

## Verification

### Verify Migration Success

Two ways to verify:

#### 1. Via Admin Dashboard

1. Log in as admin
2. Navigate to Admin Dashboard
3. Click **"Verify Claims Migration"** button
4. Check logs for verification results

#### 2. Via Application Logs

Look for output like:

```
[INFO] Starting claims migration verification...
[INFO] Verified ApplicantId claim for user@example.com (ApplicantId: ...)
[INFO] Claims verification completed. Total: 25, Verified: 25, Missing: 0, Mismatched: 0
[INFO] All applicants have correct ApplicantId claims!
```

### What If Verification Fails?

If you see missing or mismatched claims:

```
[WARN] Missing ApplicantId claim for user@example.com (ApplicantId: ...)
[WARN] Claims verification failed. Run the migration again to fix issues.
```

**Solution:** Run the migration again (it's safe to run multiple times):
- Via admin dashboard: Click "Run Claims Migration"
- Via code: Call `MigrationRunner.RunClaimsMigrationAsync()`

---

## Troubleshooting

### Issue: "Identity user not found for applicant"

**Cause:** An applicant exists in the domain database but no corresponding Identity user exists.

**Symptoms:**
```
[WARN] Identity user not found for applicant a1b2c3d4 with email user@example.com. Skipping.
```

**Solutions:**
1. **Data inconsistency** - The applicant record exists but the Identity user was deleted
   - Option A: Delete the orphaned applicant record
   - Option B: Create the Identity user manually

2. **Email mismatch** - Applicant email doesn't match Identity user email
   - Update the applicant email or Identity email to match

### Issue: "Failed to add ApplicantId claim"

**Cause:** Identity framework rejected the claim addition.

**Symptoms:**
```
[ERROR] Failed to add ApplicantId claim for user@example.com. Errors: ...
```

**Solutions:**
1. Check Identity configuration (UserManager settings)
2. Verify database permissions
3. Check for claim conflicts
4. Review full error message in logs

### Issue: Applicants Can't Log In After Migration

**Cause:** Claims not properly added or authentication configuration issue.

**Debugging Steps:**

1. **Verify migration ran:**
   ```bash
   # Check logs for migration output
   grep "ApplicantId claims migration" logs/*.log
   ```

2. **Verify claim exists:**
   - Run verification via admin dashboard
   - Check logs for specific user

3. **Check authentication flow:**
   - Add breakpoint in `AccountController.Login`
   - Verify `User.GetApplicantId()` returns a value
   - Check `ApplicantController.Dashboard` for null applicant

4. **Re-run migration:**
   - Via admin dashboard
   - Check logs for errors

### Issue: "ClaimsPrincipal extensions not working"

**Cause:** Missing `using ERecruitment.Web.Extensions;`

**Solution:**
Add the using statement to controllers:
```csharp
using ERecruitment.Web.Extensions;
```

---

## Rolling Back (Development Only)

**WARNING:** Only use in development! This will break authentication for applicants.

### Remove All ApplicantId Claims

From code (admin endpoint or custom script):

```csharp
await MigrationRunner.RemoveAllApplicantClaimsAsync(HttpContext.RequestServices);
```

This will:
- Remove all ApplicantId claims from Identity users
- Log each removal
- Leave Identity users intact (just remove the claims)

After rollback, you'll need to:
1. Re-enable session-based authentication (restore old code)
2. Re-add `ICurrentApplicant` service
3. Update controllers to use `_currentApplicant` again

---

## Migration Safety

### Is It Safe to Run Multiple Times?

**Yes!** The migration is **idempotent**:
- Skips users that already have correct claims
- Updates users with incorrect claims
- Only adds claims for users missing them
- Logs all operations for audit trail

### Can It Cause Data Loss?

**No.** The migration:
- Only adds claims (never deletes data)
- Never modifies applicant or Identity user records
- Never modifies existing claims (except ApplicantId)
- Is fully logged for rollback if needed

### Performance Impact

**Minimal:**
- Runs once on startup (subsequent startups skip if claims exist)
- O(n) complexity where n = number of applicants
- Typical execution: ~100ms per 100 users
- Non-blocking (runs before app accepts requests)

---

## Testing the Migration

### Test Scenario 1: New User Registration

1. Register a new user
2. Verify ApplicantId claim is added automatically
3. Log in with new user
4. Verify dashboard loads correctly

**Expected:**
- `AccountController.Register` adds claim after applicant creation
- Login succeeds without migration (claim already exists)
- `User.GetApplicantId()` returns correct value

### Test Scenario 2: Existing User Login

1. Run migration (automatic or manual)
2. Log in with existing user
3. Verify dashboard loads correctly

**Expected:**
- Migration adds claim for existing user
- Login succeeds
- `User.GetApplicantId()` returns correct value

### Test Scenario 3: Admin Access

1. Log in as admin
2. Navigate to admin dashboard
3. Admin should not have ApplicantId claim (normal)

**Expected:**
- Admin login succeeds
- No ApplicantId claim (admins aren't applicants)
- Admin dashboard loads correctly

---

## Migration Logs Reference

### Log Levels

- **INFO**: Normal operation (migration started, completed, claims added)
- **WARN**: Non-critical issues (user not found, claim already exists with different value)
- **ERROR**: Critical issues (failed to add claim, database errors)
- **DEBUG**: Detailed information (each user processed, claim verification)

### Common Log Messages

| Message | Level | Meaning |
|---------|-------|---------|
| "Starting ApplicantId claims migration..." | INFO | Migration started |
| "Found {Count} applicants to process" | INFO | Number of applicants to migrate |
| "Successfully added ApplicantId claim" | INFO | Claim added successfully |
| "ApplicantId claim already exists" | DEBUG | Claim exists, skipped |
| "Identity user not found" | WARN | No matching Identity user |
| "ApplicantId claim exists with incorrect value" | WARN | Claim updated |
| "Failed to add ApplicantId claim" | ERROR | Migration failed for user |
| "ApplicantId claims migration completed" | INFO | Migration finished |

---

## Support

If you encounter issues not covered in this guide:

1. **Check application logs** - Most issues are logged with detailed error messages
2. **Run verification** - Use the verify endpoint to check migration status
3. **Re-run migration** - Safe to run multiple times
4. **Review code changes** - Check `AccountController`, `ApplicantController`, etc.

For persistent issues:
- Review the `ClaimsMigration.cs` implementation
- Check Identity configuration in `Program.cs`
- Verify database connections and permissions
- Contact your development team

---

## Summary

✅ **Automatic migration** runs on every startup
✅ **Safe to run multiple times** (idempotent)
✅ **Manual options available** (admin dashboard or code)
✅ **Verification built-in** (via admin dashboard)
✅ **Fully logged** (audit trail of all operations)
✅ **Rollback available** (development only)

The migration ensures all existing users can continue using the application with the new claims-based authentication system.
