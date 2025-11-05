using ERecruitment.Web.Models;
using ERecruitment.Web.Services;

namespace ERecruitment.Web.ViewModels;

public class ApplicationsListViewModel
{
    public required IReadOnlyCollection<JobApplicationListItem> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public string? Search { get; set; }
    public ApplicationStatus? Status { get; set; }
    public Guid? JobId { get; set; }
    public IReadOnlyCollection<JobPosting> Jobs { get; set; } = Array.Empty<JobPosting>();
}


