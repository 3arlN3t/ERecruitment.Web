using ERecruitment.Web.Models;
using ERecruitment.Web.Services;

namespace ERecruitment.Web.ViewModels;

public class JobApplicationsViewModel
{
    public required JobPosting JobPosting { get; set; }
    public required IReadOnlyCollection<JobApplicationListItem> Applications { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
