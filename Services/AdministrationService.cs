using ERecruitment.Web.Models;
using ERecruitment.Web.Notifications;
using ERecruitment.Web.ViewModels;

namespace ERecruitment.Web.Services;

/// <summary>
/// Service responsible for administrative operations and analytics.
/// Bounded context: Cross-cutting admin concerns.
/// LOC Target: ~100 lines
/// </summary>
public class AdministrationService : IAdministrationService
{
    private readonly IRecruitmentRepository _repository;
    private readonly IEmailSender _emailSender;

    public AdministrationService(
        IRecruitmentRepository repository,
        IEmailSender emailSender)
    {
        _repository = repository;
        _emailSender = emailSender;
    }

    public async Task<BulkRejectResult> BulkRejectApplicationsAsync(
        AdminBulkRejectViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.SelectedApplicationIds.Count == 0)
        {
            return new BulkRejectResult(0, Array.Empty<AuditEntry>());
        }

        // Use optimized repository method (single transaction, eliminates N+1)
        var count = await _repository.BulkUpdateApplicationStatusAsync(
            model.SelectedApplicationIds,
            ApplicationStatus.Rejected,
            model.TemplateBody,
            cancellationToken);

        // Load applications with applicant info for email notifications
        var applications = await _repository.GetJobApplicationsPagedAsync(
            1,
            model.SelectedApplicationIds.Count,
            null,
            null,
            null,
            cancellationToken);

        var auditEntries = new List<AuditEntry>();

        // Send email notifications (can be optimized with background queue in future)
        foreach (var app in applications.Items.Where(a => model.SelectedApplicationIds.Contains(a.Id)))
        {
            var personalised = PersonaliseTemplate(
                model.TemplateBody,
                app.ApplicantEmail,
                app.JobTitle);

            var auditEntry = new AuditEntry
            {
                Actor = "admin",
                Action = $"Bulk rejected application {app.Id} using template",
                JobApplicationId = app.Id
            };

            auditEntries.Add(auditEntry);
            await _repository.AddAuditEntryAsync(auditEntry, cancellationToken);

            // Fire-and-forget email (notification failure shouldn't block operation)
            _ = _emailSender.SendAsync(
                app.ApplicantEmail,
                $"Application update: {app.JobTitle}",
                personalised);
        }

        return new BulkRejectResult(count, auditEntries);
    }

    public async Task<AdminDashboardModel> BuildAdminDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        // Get statistics using optimized database aggregation (O(1) memory)
        var stats = await _repository.GetDashboardStatisticsAsync(cancellationToken);

        // Get jobs (small list, acceptable to load all)
        var jobs = await _repository.GetAllJobPostingsAsync(cancellationToken);

        // Get recent applications (paginated, not all)
        var recentApplications = await _repository.GetJobApplicationsPagedAsync(
            1,
            50, // Show top 50 recent applications
            null,
            null,
            null,
            cancellationToken);

        // Get recent audit log (paginated)
        var auditLog = await _repository.GetAuditLogPagedAsync(
            1,
            100, // Show top 100 recent audit entries
            null,
            cancellationToken);

        // Get recent email deliveries (paginated)
        var emailDeliveries = await _repository.GetEmailDeliveriesPagedAsync(
            1,
            50, // Show top 50 recent emails
            cancellationToken);

        return new AdminDashboardModel(
            stats.TotalApplicants,
            stats.TotalApplications,
            stats.ActiveApplications,
            jobs,
            recentApplications.Items.Select(item => new JobApplication
            {
                Id = item.Id,
                JobTitle = item.JobTitle,
                Status = item.Status,
                SubmittedAtUtc = item.SubmittedAtUtc,
                RejectionReason = item.RejectionReason
            }).ToList(),
            stats.EquityBreakdown,
            auditLog.Items.ToList(),
            emailDeliveries.Items.ToList());
    }

    public async Task<PagedResult<JobApplicationListItem>> GetApplicationsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        ApplicationStatus? status = null,
        Guid? jobId = null,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetJobApplicationsPagedAsync(
            page,
            pageSize,
            search,
            status,
            jobId,
            cancellationToken);
    }

    public async Task<PagedResult<AuditEntry>> GetAuditLogPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAuditLogPagedAsync(page, pageSize, search, cancellationToken);
    }

    public async Task<PagedResult<Applicant>> GetApplicantsPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetApplicantsPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task<ApplicationStatusUpdateResult> UpdateApplicationStatusAsync(
        Guid applicationId,
        ApplicationStatus newStatus,
        string actor,
        string? note = null,
        string? rejectionReason = null,
        bool sendEmail = false,
        string? emailSubject = null,
        string? emailBody = null,
        CancellationToken cancellationToken = default)
    {
        var application = await _repository.FindJobApplicationByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            return new ApplicationStatusUpdateResult(false, "Application not found.", ApplicationStatus.Draft, ApplicationStatus.Draft, false);
        }

        var applicant = await _repository.FindApplicantByIdAsync(application.ApplicantId, cancellationToken);
        if (applicant is null)
        {
            return new ApplicationStatusUpdateResult(false, "Applicant record not found.", application.Status, application.Status, false);
        }

        if (newStatus == ApplicationStatus.Rejected && string.IsNullOrWhiteSpace(rejectionReason))
        {
            return new ApplicationStatusUpdateResult(false, "Please provide a rejection reason before rejecting an application.", application.Status, application.Status, false);
        }

        var previousStatus = application.Status;
        var cleanedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        var cleanedReason = string.IsNullOrWhiteSpace(rejectionReason) ? null : rejectionReason.Trim();

        application.Status = newStatus;
        if (newStatus == ApplicationStatus.Submitted && !application.SubmittedAtUtc.HasValue)
        {
            application.SubmittedAtUtc = DateTime.UtcNow;
        }

        application.RejectionReason = newStatus == ApplicationStatus.Rejected ? cleanedReason : null;

        var statusLabel = GetStatusLabel(newStatus);
        var actionBuilder = new System.Text.StringBuilder();
        if (previousStatus != newStatus)
        {
            actionBuilder.Append($"Status changed from {GetStatusLabel(previousStatus)} to {statusLabel}.");
        }
        else
        {
            actionBuilder.Append($"Status reviewed (remains {statusLabel}).");
        }

        if (!string.IsNullOrWhiteSpace(cleanedNote))
        {
            actionBuilder.Append(' ');
            actionBuilder.Append($"Note: {cleanedNote}");
        }

        if (newStatus == ApplicationStatus.Rejected && !string.IsNullOrWhiteSpace(cleanedReason))
        {
            actionBuilder.Append(' ');
            actionBuilder.Append($"Rejection reason: {cleanedReason}");
        }

        var statusAudit = new AuditEntry
        {
            Actor = actor,
            Action = actionBuilder.ToString(),
            JobApplicationId = application.Id
        };

        application.AuditTrail.Add(statusAudit);

        await _repository.UpdateJobApplicationAsync(application, cancellationToken);
        await _repository.AddAuditEntryAsync(statusAudit, cancellationToken);

        var emailSent = false;
        if (sendEmail)
        {
            var subject = string.IsNullOrWhiteSpace(emailSubject)
                ? $"Application update: {application.JobTitle}"
                : emailSubject.Trim();

            var body = string.IsNullOrWhiteSpace(emailBody)
                ? BuildDefaultEmailBody(application.JobTitle, statusLabel, cleanedReason)
                : emailBody;

            body = body.Replace("{{status}}", statusLabel, StringComparison.OrdinalIgnoreCase);

            await _emailSender.SendAsync(applicant.Email, subject, body);
            emailSent = true;

            var emailAudit = new AuditEntry
            {
                Actor = actor,
                Action = $"Notification sent to {applicant.Email} regarding status {statusLabel}.",
                JobApplicationId = application.Id
            };

            application.AuditTrail.Add(emailAudit);
            await _repository.UpdateJobApplicationAsync(application, cancellationToken);
            await _repository.AddAuditEntryAsync(emailAudit, cancellationToken);
        }

        return new ApplicationStatusUpdateResult(true, null, previousStatus, newStatus, emailSent);
    }

    // ===== PRIVATE HELPER METHODS =====

    private static string PersonaliseTemplate(string template, string applicantEmail, string jobTitle)
    {
        // Extract first name from email if possible
        var firstName = applicantEmail.Split('@').First();

        return template
            .Replace("{{firstName}}", firstName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{jobTitle}}", jobTitle, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetStatusLabel(ApplicationStatus status) => status switch
    {
        ApplicationStatus.Draft => "Draft",
        ApplicationStatus.Submitted => "Submitted",
        ApplicationStatus.Interview => "Shortlisted / Interview",
        ApplicationStatus.Offer => "Offer",
        ApplicationStatus.Rejected => "Rejected",
        ApplicationStatus.Withdrawn => "Withdrawn",
        _ => status.ToString()
    };

    private static string BuildDefaultEmailBody(string jobTitle, string statusLabel, string? rejectionReason) => $@"Dear Applicant,

We would like to update you on your application for {jobTitle}. The status of your application is now '{statusLabel}'.{(string.IsNullOrWhiteSpace(rejectionReason) ? string.Empty : $"\n\nReason provided: {rejectionReason}")}

Thank you for your interest in this opportunity.

Kind regards,
Recruitment Team";
}

/// <summary>
/// Interface for administration operations.
/// </summary>
public interface IAdministrationService
{
    Task<BulkRejectResult> BulkRejectApplicationsAsync(AdminBulkRejectViewModel model, CancellationToken cancellationToken = default);
    Task<AdminDashboardModel> BuildAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<JobApplicationListItem>> GetApplicationsPagedAsync(int page, int pageSize, string? search = null, ApplicationStatus? status = null, Guid? jobId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditEntry>> GetAuditLogPagedAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Applicant>> GetApplicantsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ApplicationStatusUpdateResult> UpdateApplicationStatusAsync(Guid applicationId, ApplicationStatus newStatus, string actor, string? note = null, string? rejectionReason = null, bool sendEmail = false, string? emailSubject = null, string? emailBody = null, CancellationToken cancellationToken = default);
}
