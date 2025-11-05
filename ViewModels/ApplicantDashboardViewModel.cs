using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class ApplicantDashboardViewModel
{
    public required Applicant Applicant { get; set; }
    public IReadOnlyCollection<JobPosting> Jobs { get; set; } = Array.Empty<JobPosting>();
    public IReadOnlyCollection<JobApplication> Applications { get; set; } = Array.Empty<JobApplication>();
}
