using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class AuditListViewModel
{
    public required IReadOnlyCollection<AuditEntry> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? Search { get; set; }
}


