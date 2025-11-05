using System.Collections.Generic;
using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

/// <summary>
/// Centralised rules for permissible job application status transitions.
/// Keeps workflow services aligned and prevents invalid state changes.
/// </summary>
internal static class ApplicationStatusRules
{
    private static readonly IReadOnlyDictionary<ApplicationStatus, HashSet<ApplicationStatus>> AllowedTransitions =
        new Dictionary<ApplicationStatus, HashSet<ApplicationStatus>>
        {
            [ApplicationStatus.Draft] = new()
            {
                ApplicationStatus.Submitted,
                ApplicationStatus.Withdrawn,
                ApplicationStatus.Rejected
            },
            [ApplicationStatus.Submitted] = new()
            {
                ApplicationStatus.Withdrawn,
                ApplicationStatus.Rejected,
                ApplicationStatus.Interview,
                ApplicationStatus.Offer
            },
            [ApplicationStatus.Interview] = new()
            {
                ApplicationStatus.Offer,
                ApplicationStatus.Rejected
            },
            [ApplicationStatus.Offer] = new()
            {
                ApplicationStatus.Rejected,
                ApplicationStatus.Withdrawn
            },
            [ApplicationStatus.Rejected] = new(),
            [ApplicationStatus.Withdrawn] = new()
        };

    /// <summary>
    /// Checks whether the application can move from the current status to the target status.
    /// </summary>
    public static bool CanTransition(ApplicationStatus current, ApplicationStatus target)
    {
        if (current == target)
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target);
    }

    /// <summary>
    /// Generates a human-readable error message for invalid transitions.
    /// </summary>
    public static string BuildErrorMessage(ApplicationStatus current, ApplicationStatus target) =>
        $"Cannot move application from {current} to {target}.";
}
