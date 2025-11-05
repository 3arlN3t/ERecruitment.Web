using System;

namespace ERecruitment.Web.Models;

public class QualificationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InstitutionName { get; set; } = string.Empty;
    public string QualificationName { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public string? YearObtained { get; set; }
    public string Status { get; set; } = "Completed"; // Completed or In Progress
}
