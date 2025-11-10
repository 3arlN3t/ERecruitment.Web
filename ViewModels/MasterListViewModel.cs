using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class MasterListViewModel
{
    public required JobPosting Job { get; init; }
    public required IReadOnlyCollection<MasterListRowViewModel> Applicants { get; init; }
    public required IReadOnlyCollection<MasterListFilterOptionViewModel> Filters { get; init; }
    public required string SelectedFilterCode { get; init; }
    public ApplicationStatus? SelectedStatus { get; init; }
    public required IReadOnlyCollection<MasterListSummaryItem> StatusSummary { get; init; }
    public required IReadOnlyCollection<MasterListSummaryItem> EquitySummary { get; init; }
    public required IReadOnlyCollection<MasterListSummaryItem> GenderSummary { get; init; }
    public required IReadOnlyCollection<MasterListSummaryItem> DisabilitySummary { get; init; }
    public int TotalCount => Applicants.Count;
}

public record MasterListRowViewModel(
    int Index,
    Guid ApplicationId,
    string ApplicantName,
    string ApplicantEmail,
    string Race,
    string DlrDisplay,
    string AgeDisplay,
    string GenderDisplay,
    string DisabilityDisplay,
    string DisabilityDetails,
    string QualificationSummary,
    string ExperienceSummary,
    string Comments,
    ApplicationStatus Status);

public record MasterListFilterOptionViewModel(
    string Code,
    string Label,
    string Description,
    ApplicationStatus? Status,
    bool IsSelected);

public record MasterListSummaryItem(
    string Label,
    int Count,
    double Percentage,
    string CssClass);
