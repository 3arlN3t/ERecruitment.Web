namespace ERecruitment.Web.Models;

public class Applicant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ApplicantProfile Profile { get; set; } = new();
    public EquityDeclaration? EquityDeclaration { get; set; }
    public List<JobApplication> Applications { get; set; } = new();
}
