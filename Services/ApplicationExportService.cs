using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ERecruitment.Web.Models;
using ERecruitment.Web.Utilities;
using ERecruitment.Web.Exceptions;
using Microsoft.Extensions.Logging;

namespace ERecruitment.Web.Services;

public class ApplicationExportService : IApplicationExportService
{
    private readonly IRecruitmentRepository _repository;
    private readonly ILogger<ApplicationExportService> _logger;

    public ApplicationExportService(IRecruitmentRepository repository, ILogger<ApplicationExportService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<FileDownloadResult?> ExportMasterListAsync(Guid jobId, string scope, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting master list. JobId: {JobId}, Scope: {Scope}", jobId, scope);

        var filter = MasterListFilterProvider.Resolve(scope);
        var job = await _repository.GetJobPostingAsync(jobId, cancellationToken);
        if (job is null)
        {
            _logger.LogWarning("Cannot export master list: job not found. JobId: {JobId}", jobId);
            return null;
        }

        var entries = await _repository.GetJobApplicationsMasterListAsync(jobId, filter.Status, cancellationToken);
        var exportRows = entries
            .Select((entry, index) =>
            {
                var timestamp = entry.SubmittedAtUtc ?? entry.CreatedAtUtc;
                var local = timestamp.ToLocalTime();
                var ageDisplay = entry.Age.HasValue ? entry.Age.Value.ToString() : string.Empty;
                var genderDisplay = string.IsNullOrWhiteSpace(entry.Gender) ? "Not declared" : entry.Gender;

                return new ExcelMasterListRow(
                    index + 1,
                    entry.ApplicantName,
                    entry.ApplicantEmail,
                    entry.Race ?? "Not declared",
                    ageDisplay,
                    genderDisplay,
                    entry.HasDisability ? "Yes" : "No",
                    entry.DisabilityNarrative ?? string.Empty,
                    entry.QualificationSummary,
                    entry.ExperienceSummary,
                    entry.Comments,
                    entry.Status.ToString(),
                    local.ToString("dd MMM yyyy HH:mm"));
            })
            .ToList();

        var workbook = ExcelXmlExporter.BuildMasterListWorkbook(
            $"{job.Title} MasterList",
            exportRows);

        var fileName = $"MasterList_{Sanitize(job.ReferenceNumber)}_{DateTime.UtcNow:yyyyMMddHHmmss}.xls";
        return new FileDownloadResult(workbook, "application/vnd.ms-excel", fileName);
    }

    /// <summary>
    /// Exports filtered job applications as CSV file.
    /// </summary>
    /// <exception cref="ResourceNotFoundException">When no applications match the filters</exception>
    public async Task<FileDownloadResult> ExportApplicationsCsvAsync(string? search = null, ApplicationStatus? status = null, Guid? jobId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting applications CSV. Search: {Search}, Status: {Status}, JobId: {JobId}",
            search, status, jobId);

        var paged = await _repository.GetJobApplicationsPagedAsync(page: 1, pageSize: int.MaxValue, search, status, jobId, cancellationToken);
        
        if (paged.Items.Count == 0)
        {
            _logger.LogWarning("CSV export requested but no applications match the filters. Search: {Search}, Status: {Status}, JobId: {JobId}",
                search, status, jobId);
            throw new ResourceNotFoundException("JobApplications", "matching the specified filters");
        }

        var sb = new StringBuilder();
        sb.AppendLine("ApplicationId,ApplicantEmail,JobTitle,Status,SubmittedAt,Outcome");
        foreach (var item in paged.Items)
        {
            var submitted = item.SubmittedAtUtc?.ToString("o") ?? string.Empty;
            var outcome = item.RejectionReason?.Replace('\n', ' ').Replace('\r', ' ') ?? string.Empty;
            sb.AppendLine($"{item.Id},{Escape(item.ApplicantEmail)},{Escape(item.JobTitle)},{item.Status},{submitted},{Escape(outcome)}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"applications_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return new FileDownloadResult(bytes, "text/csv", fileName);
    }

    private static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Job";
        }

        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var buffer = new char[value.Length];
        var index = 0;
        foreach (var ch in value)
        {
            if (!invalid.Contains(ch) && !char.IsControl(ch))
            {
                buffer[index++] = ch;
            }
        }

        var sanitized = new string(buffer, 0, index).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "Job" : sanitized;
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}

