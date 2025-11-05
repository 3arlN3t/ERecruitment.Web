using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

/// <summary>
/// Snapshot of an application enriched with applicant profile insights
/// for the administrator master list experience.
/// </summary>
public record AdminMasterListEntry(
    Guid ApplicationId,
    Guid ApplicantId,
    string ApplicantName,
    string ApplicantEmail,
    string? Race,
    int? Age,
    string Gender,
    bool HasDisability,
    string DisabilityFlag,
    string? DisabilityNarrative,
    string QualificationSummary,
    string ExperienceSummary,
    string Comments,
    ApplicationStatus Status,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc);
