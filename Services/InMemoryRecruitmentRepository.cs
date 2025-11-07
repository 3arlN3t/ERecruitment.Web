using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

/// <summary>
/// In-memory implementation of IRecruitmentRepository for testing and development.
/// NOT suitable for production use - data is lost on restart.
/// </summary>
public class InMemoryRecruitmentRepository : IRecruitmentRepository
{
    private readonly List<Applicant> _applicants = new();
    private readonly List<JobPosting> _jobPostings = new();
    private readonly List<JobApplication> _jobApplications = new();
    private readonly List<AuditEntry> _auditEntries = new();
    private readonly AuditOptions _auditOptions = new();
    private readonly List<EmailDelivery> _emailDeliveries = new();
    private readonly object _lock = new();

    // ===== APPLICANT OPERATIONS =====

    public Task<Applicant?> FindApplicantByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var applicant = _applicants.FirstOrDefault(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(applicant);
        }
    }

    public Task<Applicant?> FindApplicantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var applicant = _applicants.FirstOrDefault(a => a.Id == id);
            return Task.FromResult(applicant);
        }
    }

    public Task<PagedResult<Applicant>> GetApplicantsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var totalCount = _applicants.Count;
            var items = _applicants
                .OrderByDescending(a => a.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new PagedResult<Applicant>(items, page, pageSize, totalCount));
        }
    }

    public Task AddApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _applicants.Add(applicant);
            return Task.CompletedTask;
        }
    }

    public Task UpdateApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var existing = _applicants.FirstOrDefault(a => a.Id == applicant.Id);
            if (existing != null)
            {
                var index = _applicants.IndexOf(existing);
                _applicants[index] = applicant;
            }
            return Task.CompletedTask;
        }
    }

    // ===== JOB POSTING OPERATIONS =====

    public Task<PagedResult<JobPosting>> GetJobPostingsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var totalCount = _jobPostings.Count;
            var items = _jobPostings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new PagedResult<JobPosting>(items, page, pageSize, totalCount));
        }
    }

    public Task<IReadOnlyCollection<JobPosting>> GetAllJobPostingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var jobs = _jobPostings.ToList() as IReadOnlyCollection<JobPosting>;
            return Task.FromResult(jobs);
        }
    }

    public Task<JobPosting?> GetJobPostingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var job = _jobPostings.FirstOrDefault(j => j.Id == id);
            return Task.FromResult(job);
        }
    }

    public Task AddJobPostingAsync(JobPosting job, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _jobPostings.Add(job);
            return Task.CompletedTask;
        }
    }

    public Task UpdateJobPostingAsync(JobPosting job, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var existing = _jobPostings.FirstOrDefault(j => j.Id == job.Id);
            if (existing != null)
            {
                var index = _jobPostings.IndexOf(existing);
                _jobPostings[index] = job;
            }
            return Task.CompletedTask;
        }
    }

    public Task DeleteJobPostingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var job = _jobPostings.FirstOrDefault(j => j.Id == id);
            if (job != null)
            {
                _jobPostings.Remove(job);
            }
            return Task.CompletedTask;
        }
    }

    // ===== JOB APPLICATION OPERATIONS =====

    public Task<JobApplication?> FindJobApplicationAsync(Guid applicantId, Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var application = _jobApplications.FirstOrDefault(a =>
                a.ApplicantId == applicantId && a.JobPostingId == jobPostingId);
            return Task.FromResult(application);
        }
    }

    public Task<JobApplication?> FindJobApplicationByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var application = _jobApplications.FirstOrDefault(a => a.Id == applicationId);
            return Task.FromResult(application);
        }
    }

    public Task<IReadOnlyCollection<JobApplication>> GetApplicantApplicationsAsync(Guid applicantId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var applications = _jobApplications
                .Where(a => a.ApplicantId == applicantId)
                .ToList() as IReadOnlyCollection<JobApplication>;
            return Task.FromResult(applications);
        }
    }

    public Task AddJobApplicationAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _jobApplications.Add(application);
            return Task.CompletedTask;
        }
    }

    public Task UpdateJobApplicationAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var existing = _jobApplications.FirstOrDefault(a => a.Id == application.Id);
            if (existing != null)
            {
                var index = _jobApplications.IndexOf(existing);
                _jobApplications[index] = application;
            }
            return Task.CompletedTask;
        }
    }

    public Task<int> BulkUpdateApplicationStatusAsync(
        IEnumerable<Guid> applicationIds,
        ApplicationStatus newStatus,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var count = 0;
            var idList = applicationIds.ToList();

            foreach (var application in _jobApplications.Where(a => idList.Contains(a.Id)))
            {
                application.Status = newStatus;
                application.RejectionReason = reason;
                count++;
            }

            return Task.FromResult(count);
        }
    }

    // ===== AUDIT OPERATIONS =====

    public Task<PagedResult<AuditEntry>> GetAuditLogPagedAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _auditEntries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    (a.Actor?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.Action?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(a => a.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new PagedResult<AuditEntry>(items, page, pageSize, totalCount));
        }
    }

    public Task AddAuditEntryAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (entry.Id == Guid.Empty)
            {
                entry.Id = Guid.NewGuid();
            }

            _auditEntries.Add(entry);
            if (entry.JobApplicationId is Guid applicationId)
            {
                PruneAuditEntriesForApplication(applicationId);
            }

            return Task.CompletedTask;
        }
    }

    // ===== EMAIL DELIVERY OPERATIONS =====

    public Task<PagedResult<EmailDelivery>> GetEmailDeliveriesPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var totalCount = _emailDeliveries.Count;
            var items = _emailDeliveries
                .OrderByDescending(e => e.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new PagedResult<EmailDelivery>(items, page, pageSize, totalCount));
        }
    }

    private void PruneAuditEntriesForApplication(Guid applicationId)
    {
        if (_auditOptions.RetentionDays > 0)
        {
            var cutoff = DateTime.UtcNow.AddDays(-_auditOptions.RetentionDays);
            _auditEntries.RemoveAll(a =>
                a.JobApplicationId == applicationId &&
                a.TimestampUtc < cutoff);
        }

        if (_auditOptions.MaxEntriesPerApplication > 0)
        {
            var entries = _auditEntries
                .Where(a => a.JobApplicationId == applicationId)
                .OrderByDescending(a => a.TimestampUtc)
                .ToList();

            if (entries.Count > _auditOptions.MaxEntriesPerApplication)
            {
                var overflow = entries
                    .Skip(_auditOptions.MaxEntriesPerApplication)
                    .Select(a => a.Id)
                    .ToHashSet();

                _auditEntries.RemoveAll(a => overflow.Contains(a.Id));
            }
        }
    }

    public Task<EmailDelivery?> GetEmailDeliveryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var delivery = _emailDeliveries.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(delivery);
        }
    }

    // ===== ADMIN OPERATIONS =====

    public Task<PagedResult<JobApplicationListItem>> GetJobApplicationsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        ApplicationStatus? status = null,
        Guid? jobId = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _jobApplications.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.JobTitle.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            if (jobId.HasValue)
            {
                query = query.Where(a => a.JobPostingId == jobId.Value);
            }

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(a => a.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new JobApplicationListItem(
                    a.Id,
                    GetApplicantEmail(a.ApplicantId),
                    a.JobTitle,
                    a.Status,
                    a.SubmittedAtUtc,
                    a.RejectionReason))
                .ToList();

            return Task.FromResult(new PagedResult<JobApplicationListItem>(items, page, pageSize, totalCount));
        }
    }

    public Task<IReadOnlyCollection<AdminMasterListEntry>> GetJobApplicationsMasterListAsync(
        Guid jobId,
        ApplicationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var applications = _jobApplications
                .Where(app => app.JobPostingId == jobId && (!status.HasValue || app.Status == status.Value))
                .OrderByDescending(app => app.SubmittedAtUtc ?? app.CreatedAtUtc)
                .ToList();

            if (applications.Count == 0)
            {
                return Task.FromResult((IReadOnlyCollection<AdminMasterListEntry>)Array.Empty<AdminMasterListEntry>());
            }

            var applicantLookup = _applicants.ToDictionary(a => a.Id);
            var results = new List<AdminMasterListEntry>(applications.Count);

            foreach (var application in applications)
            {
                if (!applicantLookup.TryGetValue(application.ApplicantId, out var applicant))
                {
                    continue;
                }

                results.Add(MasterListProjection.Create(application, applicant));
            }

            return Task.FromResult((IReadOnlyCollection<AdminMasterListEntry>)results);
        }
    }

    public Task<DashboardStatistics> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var totalApplicants = _applicants.Count;
            var totalApplications = _jobApplications.Count;
            var activeApplications = _jobApplications.Count(a =>
                a.Status == ApplicationStatus.Submitted ||
                a.Status == ApplicationStatus.Interview);

            var equityBreakdown = _applicants
                .Where(a => a.EquityDeclaration?.ConsentGiven == true)
                .GroupBy(a => a.EquityDeclaration!.Ethnicity ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var stats = new DashboardStatistics(
                totalApplicants,
                totalApplications,
                activeApplications,
                equityBreakdown);

            return Task.FromResult(stats);
        }
    }

    public Task<Applicant?> GetApplicantFullProfileAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var application = _jobApplications.FirstOrDefault(a => a.Id == applicationId);
            if (application is null)
            {
                return Task.FromResult<Applicant?>(null);
            }

            var applicant = _applicants.FirstOrDefault(a => a.Id == application.ApplicantId);
            return Task.FromResult(applicant);
        }
    }

    // ===== HELPER METHODS =====

    private string GetApplicantEmail(Guid applicantId)
    {
        var applicant = _applicants.FirstOrDefault(a => a.Id == applicantId);
        return applicant?.Email ?? "Unknown";
    }
}
