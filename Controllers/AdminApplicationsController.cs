using System;
using System.IO;
using System.Text;
using ERecruitment.Web.Models;
using ERecruitment.Web.Services;
using ERecruitment.Web.Utilities;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERecruitment.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminApplicationsController : Controller
{
    private readonly IRecruitmentRepository _repo;
    private readonly IAdministrationService _adminService;

    private static readonly MasterListFilterDefinition[] MasterListFilters =
    {
        new("all", "All Applicants", "Every candidate captured for this post", null),
        new("submitted", "Submitted", "Completed submissions awaiting review", ApplicationStatus.Submitted),
        new("shortlisted", "Shortlisted / Interview", "Interview and shortlist pipeline", ApplicationStatus.Interview),
        new("offers", "Offers", "Offer and appointment stage", ApplicationStatus.Offer),
        new("rejected", "Not Suitable", "Declined or not shortlisted", ApplicationStatus.Rejected),
        new("draft", "Draft", "Applications saved but not submitted", ApplicationStatus.Draft),
        new("withdrawn", "Withdrawn", "Applications withdrawn by applicants", ApplicationStatus.Withdrawn)
    };

    public AdminApplicationsController(IRecruitmentRepository repo, IAdministrationService adminService)
    {
        _repo = repo;
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 25, string? search = null, ApplicationStatus? status = null, Guid? jobId = null)
    {
        var paged = await _repo.GetJobApplicationsPagedAsync(page, pageSize, search, status, jobId);
        var vm = new ApplicationsListViewModel
        {
            Items = paged.Items,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            Search = search,
            Status = status,
            JobId = jobId,
            Jobs = await _repo.GetAllJobPostingsAsync()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> MasterListHub()
    {
        var jobs = await _repo.GetAllJobPostingsAsync();
        var items = new List<MasterListHubItem>();

        foreach (var job in jobs.OrderByDescending(j => j.DatePosted))
        {
            var entries = await _repo.GetJobApplicationsMasterListAsync(job.Id);
            var grouped = entries
                .GroupBy(e => e.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            items.Add(new MasterListHubItem(
                job.Id,
                job.Title,
                job.ReferenceNumber,
                job.PostNumber,
                job.Centre,
                entries.Count,
                GetCount(grouped, ApplicationStatus.Submitted),
                GetCount(grouped, ApplicationStatus.Interview),
                GetCount(grouped, ApplicationStatus.Offer),
                GetCount(grouped, ApplicationStatus.Rejected),
                GetCount(grouped, ApplicationStatus.Draft),
                GetCount(grouped, ApplicationStatus.Withdrawn)));
        }

        var vm = new MasterListHubViewModel(items);
        ViewData["Title"] = "MasterList Hub";
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> MasterList(Guid jobId, string scope = "all")
    {
        var filter = ResolveMasterListFilter(scope);

        var job = await _repo.GetJobPostingAsync(jobId);
        if (job is null)
        {
            return NotFound();
        }

        var entries = await _repo.GetJobApplicationsMasterListAsync(jobId, filter.Status);
        var rows = entries
            .Select((entry, index) =>
            {
                var timeline = entry.SubmittedAtUtc ?? entry.CreatedAtUtc;
                var dlrDisplay = timeline.ToLocalTime().ToString("dd MMM yyyy");
                var ageDisplay = entry.Age.HasValue ? entry.Age.Value.ToString() : "—";

                // Abbreviate gender: F=Female, M=Male, ND=Not Declared
                var gender = string.IsNullOrWhiteSpace(entry.Gender)
                    ? "ND"
                    : entry.Gender.Trim().Equals("Female", StringComparison.OrdinalIgnoreCase)
                        ? "F"
                        : entry.Gender.Trim().Equals("Male", StringComparison.OrdinalIgnoreCase)
                            ? "M"
                            : "ND";

                // Abbreviate race: A=African, W=White, I=Indian, C=Coloured, ND=Not Declared
                var race = entry.Race == null
                    ? "ND"
                    : entry.Race.Trim().Equals("African", StringComparison.OrdinalIgnoreCase)
                        ? "A"
                        : entry.Race.Trim().Equals("White", StringComparison.OrdinalIgnoreCase)
                            ? "W"
                            : entry.Race.Trim().Equals("Indian", StringComparison.OrdinalIgnoreCase)
                                ? "I"
                                : entry.Race.Trim().Equals("Coloured", StringComparison.OrdinalIgnoreCase)
                                    ? "C"
                                    : "ND";

                // Abbreviate disability: Y=Yes, N=No, ND=Not Declared (full details for modal only)
                var disability = entry.HasDisability
                    ? "Y"
                    : string.IsNullOrWhiteSpace(entry.DisabilityNarrative)
                        ? "N"
                        : "ND";

                // Full disability details for modal
                var disabilityDetails = entry.HasDisability
                    ? string.IsNullOrWhiteSpace(entry.DisabilityNarrative)
                        ? "Yes"
                        : $"Yes ({entry.DisabilityNarrative})"
                    : (string.IsNullOrWhiteSpace(entry.DisabilityNarrative) ? "No" : entry.DisabilityNarrative!);

                return new MasterListRowViewModel(
                    index + 1,
                    entry.ApplicantName,
                    entry.ApplicantEmail,
                    race,
                    dlrDisplay,
                    ageDisplay,
                    gender,
                    disability,
                    disabilityDetails,
                    entry.QualificationSummary,
                    entry.ExperienceSummary,
                    entry.Comments,
                    entry.Status);
            })
            .ToList();

        var totalEntries = entries.Count;
        var statusSummary = BuildStatusSummary(entries, totalEntries);
        var equitySummary = BuildCategoricalSummary(entries, totalEntries, e => e.Race ?? "Not declared", "masterlist-chip-equity");
        var genderSummary = BuildCategoricalSummary(entries, totalEntries, e => string.IsNullOrWhiteSpace(e.Gender) ? "Not declared" : e.Gender!, "masterlist-chip-gender");
        var disabilitySummary = BuildCategoricalSummary(entries, totalEntries, e => e.HasDisability ? "Disability declared" : "No disability declared", "masterlist-chip-disability");

        var vm = new MasterListViewModel
        {
            Job = job,
            Applicants = rows,
            Filters = BuildFilterOptions(filter.Code),
            SelectedFilterCode = filter.Code,
            SelectedStatus = filter.Status,
            StatusSummary = statusSummary,
            EquitySummary = equitySummary,
            GenderSummary = genderSummary,
            DisabilitySummary = disabilitySummary
        };

        ViewData["Title"] = $"MasterList - {job.Title}";
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportMasterList(Guid jobId, string scope = "all")
    {
        var filter = ResolveMasterListFilter(scope);
        var job = await _repo.GetJobPostingAsync(jobId);
        if (job is null)
        {
            return NotFound();
        }

        var entries = await _repo.GetJobApplicationsMasterListAsync(jobId, filter.Status);

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
        return File(workbook, "application/vnd.ms-excel", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(string? search = null, ApplicationStatus? status = null, Guid? jobId = null)
    {
        var paged = await _repo.GetJobApplicationsPagedAsync(page: 1, pageSize: int.MaxValue, search, status, jobId);
        var sb = new StringBuilder();
        sb.AppendLine("ApplicationId,ApplicantEmail,JobTitle,Status,SubmittedAt,Outcome");
        foreach (var a in paged.Items)
        {
            var submitted = a.SubmittedAtUtc?.ToString("o") ?? string.Empty;
            var outcome = a.RejectionReason?.Replace('\n', ' ').Replace('\r', ' ') ?? string.Empty;
            sb.AppendLine($"{a.Id},{Escape(a.ApplicantEmail)},{Escape(a.JobTitle)},{a.Status},{submitted},{Escape(outcome)}");
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"applications_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var application = await _repo.FindJobApplicationByIdAsync(id);
        if (application is null)
        {
            return NotFound();
        }

        var applicant = await _repo.FindApplicantByIdAsync(application.ApplicantId);
        if (applicant is null)
        {
            return NotFound();
        }

        var viewModel = BuildStatusViewModel(application, applicant);
        viewModel.ReturnUrl = GetSafeReturnUrl(Request.Headers["Referer"], Url.Action(nameof(Index))) ?? Url.Action(nameof(Index));
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminApplicationStatusUpdateViewModel model)
    {
        if (model.NewStatus == ApplicationStatus.Rejected && string.IsNullOrWhiteSpace(model.RejectionReason))
        {
            ModelState.AddModelError(nameof(model.RejectionReason), "Please provide a rejection reason when rejecting an application.");
        }

        if (model.SendEmail && (string.IsNullOrWhiteSpace(model.EmailSubject) || string.IsNullOrWhiteSpace(model.EmailBody)))
        {
            ModelState.AddModelError(nameof(model.EmailSubject), "Email subject and body are required when sending a notification.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateStatusViewModelAsync(model);
            return View(model);
        }

        var actor = User?.Identity?.Name ?? "admin";
        var result = await _adminService.UpdateApplicationStatusAsync(
            model.ApplicationId,
            model.NewStatus,
            actor,
            model.Note,
            model.RejectionReason,
            model.SendEmail,
            model.EmailSubject,
            model.EmailBody,
            HttpContext.RequestAborted);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update application status.");
            await PopulateStatusViewModelAsync(model);
            return View(model);
        }

        var statusLabel = MapStatusLabel(model.NewStatus);
        TempData["Flash"] = result.EmailSent
            ? $"Application status updated to {statusLabel} and notification sent to the applicant."
            : $"Application status updated to {statusLabel}.";

        var redirectUrl = GetSafeReturnUrl(model.ReturnUrl, Url.Action(nameof(Index))) ?? Url.Action(nameof(Index));
        return Redirect(redirectUrl!);
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }

    private MasterListFilterDefinition ResolveMasterListFilter(string scope)
    {
        var filter = MasterListFilters.FirstOrDefault(f =>
            f.Code.Equals(scope ?? string.Empty, StringComparison.OrdinalIgnoreCase));

        return filter ?? MasterListFilters[0];
    }

    private IReadOnlyCollection<MasterListFilterOptionViewModel> BuildFilterOptions(string selectedCode)
    {
        return MasterListFilters
            .Select(f => new MasterListFilterOptionViewModel(
                f.Code,
                f.Label,
                f.Description,
                f.Status,
                string.Equals(f.Code, selectedCode, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private AdminApplicationStatusUpdateViewModel BuildStatusViewModel(JobApplication application, Applicant applicant)
    {
        var viewModel = new AdminApplicationStatusUpdateViewModel
        {
            ApplicationId = application.Id,
            ApplicantEmail = applicant.Email,
            ApplicantName = applicant.Profile?.FirstName ?? applicant.Email,
            JobTitle = application.JobTitle,
            CurrentStatus = application.Status,
            NewStatus = application.Status,
            RejectionReason = application.RejectionReason,
            StatusOptions = BuildStatusOptions(application.Status),
            SubmittedAtUtc = application.SubmittedAtUtc,
            AuditTrail = application.AuditTrail
                .OrderByDescending(a => a.TimestampUtc)
                .ToList(),
            EmailSubject = $"Application update: {application.JobTitle}",
            EmailBody = $"Dear {{applicantName}},\\n\\nWe would like to update you that your application for {application.JobTitle} is now '{{{{status}}}}'.\\n\\nKind regards,\\nRecruitment Team"
        };

        return viewModel;
    }

    private async Task PopulateStatusViewModelAsync(AdminApplicationStatusUpdateViewModel model)
    {
        var application = await _repo.FindJobApplicationByIdAsync(model.ApplicationId);
        if (application is null)
        {
            return;
        }

        var applicant = await _repo.FindApplicantByIdAsync(application.ApplicantId);
        if (applicant is null)
        {
            return;
        }

        model.ApplicantEmail = applicant.Email;
        model.ApplicantName = applicant.Profile?.FirstName ?? applicant.Email;
        model.JobTitle = application.JobTitle;
        model.CurrentStatus = application.Status;
        model.StatusOptions = BuildStatusOptions(model.NewStatus);
        model.SubmittedAtUtc = application.SubmittedAtUtc;
        model.AuditTrail = application.AuditTrail
            .OrderByDescending(a => a.TimestampUtc)
            .ToList();

        if (model.NewStatus == ApplicationStatus.Rejected && string.IsNullOrWhiteSpace(model.RejectionReason))
        {
            model.RejectionReason = application.RejectionReason;
        }

        if (string.IsNullOrWhiteSpace(model.EmailSubject))
        {
            model.EmailSubject = $"Application update: {application.JobTitle}";
        }

        if (string.IsNullOrWhiteSpace(model.EmailBody))
        {
            model.EmailBody = $"Dear {{applicantName}},\\n\\nWe would like to update you that your application for {application.JobTitle} is now '{{{{status}}}}'.\\n\\nKind regards,\\nRecruitment Team";
        }
    }

    private static IEnumerable<SelectListItem> BuildStatusOptions(ApplicationStatus selected)
    {
        var options = new[]
        {
            ApplicationStatus.Submitted,
            ApplicationStatus.Interview,
            ApplicationStatus.Offer,
            ApplicationStatus.Rejected,
            ApplicationStatus.Withdrawn
        };

        return options.Select(status => new SelectListItem
        {
            Text = MapStatusLabel(status),
            Value = status.ToString(),
            Selected = status == selected
        }).ToList();
    }

    private static string? GetSafeReturnUrl(string? candidate, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(candidate) && Uri.TryCreate(candidate, UriKind.RelativeOrAbsolute, out var uri))
        {
            if (!uri.IsAbsoluteUri)
            {
                return candidate;
            }

            if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return uri.PathAndQuery;
            }
        }

        return fallback;
    }

    private static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Job";
        }

        var invalid = Path.GetInvalidFileNameChars();
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

    private IReadOnlyCollection<MasterListSummaryItem> BuildStatusSummary(IReadOnlyCollection<AdminMasterListEntry> entries, int total)
    {
        if (total == 0)
        {
            return Array.Empty<MasterListSummaryItem>();
        }

        return entries
            .GroupBy(e => e.Status)
            .OrderByDescending(g => g.Count())
            .Select(g => new MasterListSummaryItem(
                Label: MapStatusLabel(g.Key),
                Count: g.Count(),
                Percentage: Math.Round(g.Count() * 100d / total, 1, MidpointRounding.AwayFromZero),
                CssClass: $"masterlist-chip-status masterlist-status-{g.Key.ToString().ToLowerInvariant()}"))
            .ToList();
    }

    private IReadOnlyCollection<MasterListSummaryItem> BuildCategoricalSummary(
        IReadOnlyCollection<AdminMasterListEntry> entries,
        int total,
        Func<AdminMasterListEntry, string> selector,
        string cssClass)
    {
        if (total == 0)
        {
            return Array.Empty<MasterListSummaryItem>();
        }

        return entries
            .GroupBy(selector)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .Select(g => new MasterListSummaryItem(
                Label: g.Key,
                Count: g.Count(),
                Percentage: Math.Round(g.Count() * 100d / total, 1, MidpointRounding.AwayFromZero),
                CssClass: cssClass))
            .ToList();
    }

    private static string MapStatusLabel(ApplicationStatus status) => status switch
    {
        ApplicationStatus.Draft => "Draft",
        ApplicationStatus.Submitted => "Submitted",
        ApplicationStatus.Interview => "Shortlisted / Interview",
        ApplicationStatus.Offer => "Offer",
        ApplicationStatus.Rejected => "Not Suitable",
        ApplicationStatus.Withdrawn => "Withdrawn",
        _ => status.ToString()
    };

    private static int GetCount(IReadOnlyDictionary<ApplicationStatus, int> source, ApplicationStatus status) =>
        source.TryGetValue(status, out var count) ? count : 0;

    /// <summary>
    /// Bulk update application statuses
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpdateStatus(List<Guid> applicationIds, ApplicationStatus newStatus, string? note = null)
    {
        if (applicationIds == null || applicationIds.Count == 0)
        {
            TempData["Error"] = "No applications selected.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var count = await _repo.BulkUpdateApplicationStatusAsync(
                applicationIds,
                newStatus,
                note);

            TempData["Success"] = $"Successfully updated {count} application(s) to status: {newStatus}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error updating applications: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private record MasterListFilterDefinition(
        string Code,
        string Label,
        string Description,
        ApplicationStatus? Status);

    public record MasterListHubViewModel(IReadOnlyCollection<MasterListHubItem> Jobs);

    public record MasterListHubItem(
        Guid JobId,
        string Title,
        string ReferenceNumber,
        string PostNumber,
        string Centre,
        int Total,
        int Submitted,
        int Shortlisted,
        int Offer,
        int NotSuitable,
        int Draft,
        int Withdrawn);
}
