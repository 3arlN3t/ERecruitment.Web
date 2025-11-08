using ERecruitment.Web.Models;
using ERecruitment.Web.ViewModels;

namespace ERecruitment.Web.Services;

public interface IProfileMapper
{
    AdminApplicationStatusUpdateViewModel CreateStatusUpdateViewModel(JobApplication application, Applicant applicant);
    void UpdateStatusViewModel(AdminApplicationStatusUpdateViewModel model, JobApplication application, Applicant applicant);
    AdminApplicantProfileViewModel MapApplicantProfile(Applicant applicant, JobApplication application);
}

