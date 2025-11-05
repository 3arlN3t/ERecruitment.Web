using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class ApplicationTimelineViewModel
{
    public required JobApplication Application { get; set; }
    public required JobPosting Job { get; set; }
    public IReadOnlyList<AuditEntry> Events { get; set; } = Array.Empty<AuditEntry>();
}


