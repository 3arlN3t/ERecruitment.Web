using ERecruitment.Web.Models;
using ERecruitment.Web.ViewModels;

namespace ERecruitment.Web.Services;

public interface IApplicationService
{
    RegistrationResult RegisterApplicant(RegisterViewModel model);
    AuthenticationResult Authenticate(LoginViewModel model);
    Applicant? GetApplicant(Guid id);
    Applicant? FindApplicantByEmail(string email);
    Applicant? GetApplicantForSession(ISession session);
    void SetApplicantSession(ISession session, Guid applicantId);
    void ClearApplicantSession(ISession session);
    Task<ProfileUpdateResult> UpdateProfileAsync(Applicant applicant, ProfileViewModel model);
    JobPosting? GetJobPosting(Guid id);
    IReadOnlyCollection<JobPosting> GetJobs();
    ApplicationFlowResult StartApplication(Applicant applicant, Guid jobId);
    Task<ApplicationFlowResult> SubmitDirectApplication(Applicant applicant, Guid jobId);
    Task<ApplicationFlowResult> SubmitKillerQuestion(Applicant applicant, Guid jobId, string answer, bool pass, int questionIndex, bool saveAsDraft);
    Task<ApplicationFlowResult> SubmitScreenedApplication(Applicant applicant, Guid jobId);
    ApplicationFlowResult WithdrawApplication(Applicant applicant, Guid jobId, string? reason);
    IReadOnlyCollection<JobApplication> GetApplications(Applicant applicant);
    BulkRejectResult BulkRejectApplications(AdminBulkRejectViewModel model);
    AdminDashboardModel BuildAdminDashboard();
}
