namespace ERecruitment.Web.Models;

public class JobApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicantId { get; set; }
    public Guid JobPostingId { get; set; }
    public required string JobTitle { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }
    public List<ScreeningAnswer> ScreeningAnswers { get; set; } = new();
    public string? RejectionReason { get; set; }
    public List<AuditEntry> AuditTrail { get; set; } = new();
}
