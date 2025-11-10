using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class ApplicationStartViewModel
{
    public required JobPosting Job { get; set; }
    public JobApplication? ExistingApplication { get; set; }
}
