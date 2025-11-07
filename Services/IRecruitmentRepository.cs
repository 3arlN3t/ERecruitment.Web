using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

/// <summary>
/// Repository interface for recruitment system data access.
/// All operations are asynchronous to prevent thread pool exhaustion.
/// </summary>
public interface IRecruitmentRepository
{
    // ===== APPLICANT OPERATIONS =====

    Task<Applicant?> FindApplicantByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Applicant?> FindApplicantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Applicant>> GetApplicantsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default);
    Task UpdateApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default);

    // ===== JOB POSTING OPERATIONS =====

    Task<PagedResult<JobPosting>> GetJobPostingsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<JobPosting>> GetAllJobPostingsAsync(CancellationToken cancellationToken = default);
    Task<JobPosting?> GetJobPostingAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddJobPostingAsync(JobPosting job, CancellationToken cancellationToken = default);
    Task UpdateJobPostingAsync(JobPosting job, CancellationToken cancellationToken = default);
    Task DeleteJobPostingAsync(Guid id, CancellationToken cancellationToken = default);

    // ===== JOB APPLICATION OPERATIONS =====

    Task<JobApplication?> FindJobApplicationAsync(Guid applicantId, Guid jobPostingId, CancellationToken cancellationToken = default);
    Task<JobApplication?> FindJobApplicationByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<JobApplication>> GetApplicantApplicationsAsync(Guid applicantId, CancellationToken cancellationToken = default);
    Task AddJobApplicationAsync(JobApplication application, CancellationToken cancellationToken = default);
    Task UpdateJobApplicationAsync(JobApplication application, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimized bulk update for rejection workflow.
    /// Single transaction, single database round trip.
    /// </summary>
    Task<int> BulkUpdateApplicationStatusAsync(
        IEnumerable<Guid> applicationIds,
        ApplicationStatus newStatus,
        string? reason,
        CancellationToken cancellationToken = default);

    // ===== AUDIT OPERATIONS =====

    Task<PagedResult<AuditEntry>> GetAuditLogPagedAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task AddAuditEntryAsync(AuditEntry entry, CancellationToken cancellationToken = default);

    // ===== EMAIL DELIVERY OPERATIONS =====

    Task<PagedResult<EmailDelivery>> GetEmailDeliveriesPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<EmailDelivery?> GetEmailDeliveryAsync(Guid id, CancellationToken cancellationToken = default);

    // ===== ADMIN OPERATIONS =====

    Task<PagedResult<JobApplicationListItem>> GetJobApplicationsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        ApplicationStatus? status = null,
        Guid? jobId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AdminMasterListEntry>> GetJobApplicationsMasterListAsync(
        Guid jobId,
        ApplicationStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dashboard statistics using optimized database aggregation.
    /// Does NOT load all applicants into memory.
    /// </summary>
    Task<DashboardStatistics> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets full applicant profile with all related data for admin viewing.
    /// Includes profile, equity declaration, applications, and all documents.
    /// </summary>
    Task<Applicant?> GetApplicantFullProfileAsync(Guid applicationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dashboard statistics computed via database aggregation (not in-memory).
/// </summary>
public record DashboardStatistics(
    int TotalApplicants,
    int TotalApplications,
    int ActiveApplications,
    Dictionary<string, int> EquityBreakdown);
