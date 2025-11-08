using System.Security.Claims;
using ERecruitment.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERecruitment.Web.Data;

/// <summary>
/// Migrates existing Identity users to include ApplicantId claims.
/// This is a one-time migration needed when switching from session-based to claims-based authentication.
/// </summary>
public class ClaimsMigration
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ClaimsMigration> _logger;

    public ClaimsMigration(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager,
        ILogger<ClaimsMigration> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Migrates all existing applicants to have ApplicantId claims in their Identity user accounts.
    /// Safe to run multiple times - will skip users that already have the claim.
    /// </summary>
    public async Task MigrateApplicantClaimsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ApplicantId claims migration...");

        var applicants = await _dbContext.Applicants
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!applicants.Any())
        {
            _logger.LogInformation("No applicants found in database. Migration skipped.");
            return;
        }

        _logger.LogInformation("Found {Count} applicants to process", applicants.Count);

        var migratedCount = 0;
        var skippedCount = 0;
        var errorCount = 0;

        foreach (var applicant in applicants)
        {
            try
            {
                var result = await MigrateApplicantClaimAsync(applicant, cancellationToken);

                if (result == MigrationResult.Added)
                {
                    migratedCount++;
                }
                else if (result == MigrationResult.AlreadyExists)
                {
                    skippedCount++;
                }
                else if (result == MigrationResult.UserNotFound)
                {
                    _logger.LogWarning(
                        "Identity user not found for applicant {ApplicantId} with email {Email}. Skipping.",
                        applicant.Id,
                        applicant.Email);
                    errorCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error migrating claim for applicant {ApplicantId} with email {Email}",
                    applicant.Id,
                    applicant.Email);
                errorCount++;
            }
        }

        _logger.LogInformation(
            "ApplicantId claims migration completed. Added: {Added}, Skipped: {Skipped}, Errors: {Errors}",
            migratedCount,
            skippedCount,
            errorCount);
    }

    /// <summary>
    /// Migrates a single applicant's claim.
    /// </summary>
    private async Task<MigrationResult> MigrateApplicantClaimAsync(
        Applicant applicant,
        CancellationToken cancellationToken)
    {
        // Find the corresponding Identity user
        var identityUser = await _userManager.FindByEmailAsync(applicant.Email);
        if (identityUser == null)
        {
            return MigrationResult.UserNotFound;
        }

        // Check if claim already exists
        var existingClaims = await _userManager.GetClaimsAsync(identityUser);
        var applicantIdClaim = existingClaims.FirstOrDefault(c => c.Type == "ApplicantId");

        if (applicantIdClaim != null)
        {
            // Verify the claim value is correct
            if (applicantIdClaim.Value == applicant.Id.ToString())
            {
                _logger.LogDebug(
                    "ApplicantId claim already exists for {Email}. Skipping.",
                    applicant.Email);
                return MigrationResult.AlreadyExists;
            }

            // Claim exists but with wrong value - remove and re-add
            _logger.LogWarning(
                "ApplicantId claim exists with incorrect value for {Email}. Updating from {OldValue} to {NewValue}",
                applicant.Email,
                applicantIdClaim.Value,
                applicant.Id);

            await _userManager.RemoveClaimAsync(identityUser, applicantIdClaim);
        }

        // Add the ApplicantId claim
        var newClaim = new Claim("ApplicantId", applicant.Id.ToString());
        var result = await _userManager.AddClaimAsync(identityUser, newClaim);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "Successfully added ApplicantId claim for {Email} (ApplicantId: {ApplicantId})",
                applicant.Email,
                applicant.Id);
            return MigrationResult.Added;
        }

        _logger.LogError(
            "Failed to add ApplicantId claim for {Email}. Errors: {Errors}",
            applicant.Email,
            string.Join(", ", result.Errors.Select(e => e.Description)));

        throw new InvalidOperationException(
            $"Failed to add ApplicantId claim for {applicant.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    private enum MigrationResult
    {
        Added,
        AlreadyExists,
        UserNotFound
    }
}
