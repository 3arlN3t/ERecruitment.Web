using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

public record RegistrationResult(bool Success, string? ErrorMessage, Applicant? Applicant = null);
public record AuthenticationResult(bool Success, string? ErrorMessage, Applicant? Applicant = null);
public record ProfileUpdateResult(bool Success, string? ErrorMessage);
public record ApplicationFlowResult(bool Success, string? ErrorMessage, JobApplication? Application = null, bool QuestionPassed = false);
public record BulkRejectResult(int UpdatedCount, IReadOnlyCollection<AuditEntry> AuditEntries);

public record AdminDashboardModel(
    int TotalApplicants,
    int TotalApplications,
    int ActiveApplications,
    IReadOnlyCollection<JobPosting> Jobs,
    IReadOnlyCollection<JobApplication> Applications,
    IReadOnlyDictionary<string, int> EquityOptInSummary,
    IReadOnlyCollection<AuditEntry> AuditLog,
    IReadOnlyCollection<EmailDelivery> EmailDeliveries);

public record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
};

public record JobApplicationListItem(
    Guid Id,
    string ApplicantEmail,
    string JobTitle,
    ApplicationStatus Status,
    DateTime? SubmittedAtUtc,
    string? RejectionReason);

public record ApplicationStatusUpdateResult(
    bool Success,
    string? ErrorMessage,
    ApplicationStatus PreviousStatus,
    ApplicationStatus CurrentStatus,
    bool EmailSent);
