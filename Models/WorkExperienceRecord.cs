namespace ERecruitment.Web.Models;

public class WorkExperienceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployerName { get; set; } = string.Empty;
    public string PositionHeld { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Status { get; set; } = string.Empty; // Current, Resigned, etc.
    public string? ReasonForLeaving { get; set; }
}
