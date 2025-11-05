namespace ERecruitment.Web.Services;

/// <summary>
/// Configuration options for audit trail retention.
/// </summary>
public class AuditOptions
{
    /// <summary>
    /// Maximum number of audit entries retained per job application.
    /// Use 0 or a negative value to disable count-based pruning.
    /// </summary>
    public int MaxEntriesPerApplication { get; init; } = 200;

    /// <summary>
    /// Number of days to retain audit entries. Entries older than this threshold are pruned.
    /// Use 0 or a negative value to disable time-based pruning.
    /// </summary>
    public int RetentionDays { get; init; } = 365;
}
