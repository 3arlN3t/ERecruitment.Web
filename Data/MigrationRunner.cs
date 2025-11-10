using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERecruitment.Web.Data;

/// <summary>
/// Standalone migration runner for manual execution.
/// Can be called from command line or as a separate process.
/// </summary>
public static class MigrationRunner
{
    /// <summary>
    /// Runs the ApplicantId claims migration.
    /// This can be called manually if you want to skip automatic migration on startup.
    /// </summary>
    /// <param name="services">The application service provider</param>
    public static async Task RunClaimsMigrationAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ClaimsMigration>>();

        var migration = new ClaimsMigration(dbContext, userManager, logger);
        await migration.MigrateApplicantClaimsAsync();
    }

    /// <summary>
    /// Verifies that all applicants have corresponding ApplicantId claims.
    /// Useful for debugging or verification after migration.
    /// </summary>
    public static async Task VerifyClaimsMigrationAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ClaimsMigration>>();

        logger.LogInformation("Starting claims migration verification...");

        var applicants = await dbContext.Applicants.AsNoTracking().ToListAsync();

        if (!applicants.Any())
        {
            logger.LogInformation("No applicants found in database.");
            return;
        }

        var verified = 0;
        var missing = 0;
        var mismatched = 0;

        foreach (var applicant in applicants)
        {
            var identityUser = await userManager.FindByEmailAsync(applicant.Email);
            if (identityUser == null)
            {
                logger.LogWarning(
                    "No Identity user found for applicant {Email} (ApplicantId: {ApplicantId})",
                    applicant.Email,
                    applicant.Id);
                missing++;
                continue;
            }

            var claims = await userManager.GetClaimsAsync(identityUser);
            var applicantIdClaim = claims.FirstOrDefault(c => c.Type == "ApplicantId");

            if (applicantIdClaim == null)
            {
                logger.LogWarning(
                    "Missing ApplicantId claim for {Email} (ApplicantId: {ApplicantId})",
                    applicant.Email,
                    applicant.Id);
                missing++;
            }
            else if (applicantIdClaim.Value != applicant.Id.ToString())
            {
                logger.LogWarning(
                    "Mismatched ApplicantId claim for {Email}. Expected: {Expected}, Actual: {Actual}",
                    applicant.Email,
                    applicant.Id,
                    applicantIdClaim.Value);
                mismatched++;
            }
            else
            {
                logger.LogDebug(
                    "Verified ApplicantId claim for {Email} (ApplicantId: {ApplicantId})",
                    applicant.Email,
                    applicant.Id);
                verified++;
            }
        }

        logger.LogInformation(
            "Claims verification completed. Total: {Total}, Verified: {Verified}, Missing: {Missing}, Mismatched: {Mismatched}",
            applicants.Count,
            verified,
            missing,
            mismatched);

        if (missing > 0 || mismatched > 0)
        {
            logger.LogWarning(
                "Claims migration verification failed. Run the migration again to fix issues.");
        }
        else
        {
            logger.LogInformation("All applicants have correct ApplicantId claims!");
        }
    }

    /// <summary>
    /// Removes all ApplicantId claims (for testing/rollback purposes only).
    /// WARNING: This will break authentication for applicants!
    /// </summary>
    public static async Task RemoveAllApplicantClaimsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ClaimsMigration>>();

        logger.LogWarning("DANGER: Removing all ApplicantId claims. This should only be used for testing!");

        var applicants = await dbContext.Applicants.AsNoTracking().ToListAsync();
        var removedCount = 0;

        foreach (var applicant in applicants)
        {
            var identityUser = await userManager.FindByEmailAsync(applicant.Email);
            if (identityUser == null) continue;

            var claims = await userManager.GetClaimsAsync(identityUser);
            var applicantIdClaim = claims.FirstOrDefault(c => c.Type == "ApplicantId");

            if (applicantIdClaim != null)
            {
                await userManager.RemoveClaimAsync(identityUser, applicantIdClaim);
                removedCount++;
                logger.LogInformation("Removed ApplicantId claim for {Email}", applicant.Email);
            }
        }

        logger.LogWarning("Removed {Count} ApplicantId claims", removedCount);
    }
}
