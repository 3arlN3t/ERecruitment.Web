using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class BrowseJobsViewModel
{
    public IReadOnlyCollection<JobPosting> Jobs { get; set; } = Array.Empty<JobPosting>();
    public IReadOnlyCollection<JobApplication> ApplicantApplications { get; set; } = Array.Empty<JobApplication>();
    public bool IsApplicant { get; set; }
    public bool IsAdmin { get; set; }
    public bool ProfileReadyForApplications { get; set; }
    public IReadOnlyCollection<string> MissingProfileFields { get; set; } = Array.Empty<string>();
}
