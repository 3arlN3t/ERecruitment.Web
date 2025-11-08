using System.Security.Claims;
using ERecruitment.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERecruitment.Web.Data;

/// <summary>
/// Fixes orphaned applicants by creating corresponding Identity users.
/// An orphaned applicant is one that exists in the domain database but has no Identity user.
/// </summary>
public class OrphanedApplicantFixer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<OrphanedApplicantFixer> _logger;

    public OrphanedApplicantFixer(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager,
        ILogger<OrphanedApplicantFixer> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Finds and fixes all orphaned applicants by creating Identity users for them.
    /// </summary>
    /// <param name="defaultPassword">Default password for created Identity users. Use a strong password!</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of orphaned applicants fixed</returns>
    public async Task<OrphanedApplicantFixResult> FixOrphanedApplicantsAsync(
        string defaultPassword = "Test1234!",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting orphaned applicant fix process...");

        var applicants = await _dbContext.Applicants
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!applicants.Any())
        {
            _logger.LogInformation("No applicants found in database.");
            return new OrphanedApplicantFixResult(0, 0, 0, new List<string>());
        }

        _logger.LogInformation("Found {Count} applicants to check for orphaned records", applicants.Count);

        var orphanedApplicants = new List<Applicant>();
        var skippedApplicants = 0;

        // Find orphaned applicants (no corresponding Identity user)
        foreach (var applicant in applicants)
        {
            var identityUser = await _userManager.FindByEmailAsync(applicant.Email);
            if (identityUser == null)
            {
                orphanedApplicants.Add(applicant);
                _logger.LogWarning(
                    "Found orphaned applicant: {Email} (ApplicantId: {ApplicantId})",
                    applicant.Email,
                    applicant.Id);
            }
            else
            {
                skippedApplicants++;
            }
        }

        if (!orphanedApplicants.Any())
        {
            _logger.LogInformation("No orphaned applicants found. All applicants have Identity users.");
            return new OrphanedApplicantFixResult(0, skippedApplicants, 0, new List<string>());
        }

        _logger.LogInformation(
            "Found {OrphanedCount} orphaned applicants out of {TotalCount} total",
            orphanedApplicants.Count,
            applicants.Count);

        // Fix orphaned applicants
        var fixedCount = 0;
        var errorCount = 0;
        var fixedEmails = new List<string>();

        foreach (var applicant in orphanedApplicants)
        {
            try
            {
                var result = await CreateIdentityUserForApplicantAsync(applicant, defaultPassword, cancellationToken);
                if (result)
                {
                    fixedCount++;
                    fixedEmails.Add(applicant.Email);
                    _logger.LogInformation(
                        "Successfully created Identity user for {Email} with ApplicantId claim",
                        applicant.Email);
                }
                else
                {
                    errorCount++;
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogError(ex,
                    "Error creating Identity user for applicant {Email} (ApplicantId: {ApplicantId})",
                    applicant.Email,
                    applicant.Id);
            }
        }

        _logger.LogInformation(
            "Orphaned applicant fix completed. Fixed: {Fixed}, Skipped: {Skipped}, Errors: {Errors}",
            fixedCount,
            skippedApplicants,
            errorCount);

        return new OrphanedApplicantFixResult(fixedCount, skippedApplicants, errorCount, fixedEmails);
    }

    /// <summary>
    /// Creates an Identity user for a specific orphaned applicant.
    /// </summary>
    private async Task<bool> CreateIdentityUserForApplicantAsync(
        Applicant applicant,
        string defaultPassword,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Double-check that user doesn't already exist
        var existingUser = await _userManager.FindByEmailAsync(applicant.Email);
        if (existingUser != null)
        {
            _logger.LogWarning(
                "Identity user already exists for {Email}. Skipping creation but adding claim if needed.",
                applicant.Email);

            // Make sure claim exists
            var claims = await _userManager.GetClaimsAsync(existingUser);
            var hasApplicantIdClaim = claims.Any(c => c.Type == "ApplicantId" && c.Value == applicant.Id.ToString());

            if (!hasApplicantIdClaim)
            {
                await _userManager.AddClaimAsync(existingUser, new Claim("ApplicantId", applicant.Id.ToString()));
                _logger.LogInformation("Added missing ApplicantId claim to existing user {Email}", applicant.Email);
            }

            return true;
        }

        // Create new Identity user
        var identityUser = new IdentityUser
        {
            UserName = applicant.Email,
            Email = applicant.Email,
            EmailConfirmed = true // Auto-confirm for development/migrated users
        };

        var result = await _userManager.CreateAsync(identityUser, defaultPassword);

        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to create Identity user for {Email}. Errors: {Errors}",
                applicant.Email,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        // Add ApplicantId claim
        var claimResult = await _userManager.AddClaimAsync(
            identityUser,
            new Claim("ApplicantId", applicant.Id.ToString()));

        if (!claimResult.Succeeded)
        {
            _logger.LogError(
                "Failed to add ApplicantId claim for {Email}. Errors: {Errors}",
                applicant.Email,
                string.Join(", ", claimResult.Errors.Select(e => e.Description)));

            // User was created but claim failed - still count as partial success
            _logger.LogWarning(
                "Identity user created for {Email} but claim addition failed. Run claims migration to fix.",
                applicant.Email);
        }

        return true;
    }

    /// <summary>
    /// Gets a list of all orphaned applicants (applicants without Identity users).
    /// </summary>
    public async Task<List<OrphanedApplicantInfo>> GetOrphanedApplicantsAsync(
        CancellationToken cancellationToken = default)
    {
        var applicants = await _dbContext.Applicants
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var orphanedList = new List<OrphanedApplicantInfo>();

        foreach (var applicant in applicants)
        {
            var identityUser = await _userManager.FindByEmailAsync(applicant.Email);
            if (identityUser == null)
            {
                orphanedList.Add(new OrphanedApplicantInfo(
                    applicant.Id,
                    applicant.Email,
                    applicant.Profile?.FirstName ?? "Unknown",
                    applicant.Profile?.LastName ?? "Unknown",
                    applicant.CreatedAtUtc));
            }
        }

        return orphanedList;
    }
}

/// <summary>
/// Result of the orphaned applicant fix operation.
/// </summary>
public record OrphanedApplicantFixResult(
    int FixedCount,
    int SkippedCount,
    int ErrorCount,
    List<string> FixedEmails)
{
    public int TotalProcessed => FixedCount + SkippedCount + ErrorCount;
}

/// <summary>
/// Information about an orphaned applicant.
/// </summary>
public record OrphanedApplicantInfo(
    Guid ApplicantId,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAtUtc);
