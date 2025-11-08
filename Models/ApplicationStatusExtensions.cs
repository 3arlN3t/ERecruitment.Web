using System;
using System.Collections.Generic;
using System.Linq;

namespace ERecruitment.Web.Models;

public static class ApplicationStatusExtensions
{
    public static readonly ApplicationStatus[] PipelineStatuses =
    {
        ApplicationStatus.Submitted,
        ApplicationStatus.Interview,
        ApplicationStatus.Offer
    };

    public static bool IsPipelineStatus(this ApplicationStatus status) => PipelineStatuses.Contains(status);

    public static bool IsPipelineStatus(this ApplicationStatus? status) => status.HasValue && status.Value.IsPipelineStatus();

    public static IReadOnlyCollection<ApplicationStatus> PipelineStatusSet => PipelineStatuses;

    public static string ToDisplayLabel(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.Draft => "Draft",
        ApplicationStatus.Submitted => "Submitted",
        ApplicationStatus.Interview => "Shortlisted / Interview",
        ApplicationStatus.Offer => "Offer",
        ApplicationStatus.Rejected => "Not Suitable",
        ApplicationStatus.Withdrawn => "Withdrawn",
        _ => status.ToString()
    };
}

