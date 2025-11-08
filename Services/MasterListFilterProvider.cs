using System;
using System.Collections.Generic;
using System.Linq;
using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

public record MasterListFilterDefinition(string Code, string Label, string Description, ApplicationStatus? Status);

public static class MasterListFilterProvider
{
    private static readonly MasterListFilterDefinition[] _filters =
    {
        new("all", "All Applicants", "Every candidate captured for this post", null),
        new("submitted", "Submitted", "Completed submissions awaiting review", ApplicationStatus.Submitted),
        new("shortlisted", "Shortlisted / Interview", "Interview and shortlist pipeline", ApplicationStatus.Interview),
        new("offers", "Offers", "Offer and appointment stage", ApplicationStatus.Offer),
        new("rejected", "Not Suitable", "Declined or not shortlisted", ApplicationStatus.Rejected),
        new("draft", "Draft", "Applications saved but not submitted", ApplicationStatus.Draft),
        new("withdrawn", "Withdrawn", "Applications withdrawn by applicants", ApplicationStatus.Withdrawn)
    };

    public static IReadOnlyList<MasterListFilterDefinition> Filters => _filters;

    public static MasterListFilterDefinition Resolve(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return _filters[0];
        }

        return _filters.FirstOrDefault(f => f.Code.Equals(scope, StringComparison.OrdinalIgnoreCase)) ?? _filters[0];
    }
}

