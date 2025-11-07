using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class AdminApplicantProfileViewModel
{
    // Application Context
    public Guid ApplicationId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public ApplicationStatus ApplicationStatus { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Applicant Basic Info
    public Guid ApplicantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Personal Information
    public DateTime? DateOfBirth { get; set; }
    public int? Age => DateOfBirth.HasValue
        ? DateTime.Today.Year - DateOfBirth.Value.Year - (DateTime.Today.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
        : null;
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? SaIdNumber { get; set; }
    public string? PassportNumber { get; set; }
    public bool IsSouthAfrican { get; set; }
    public string? Nationality { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? ContactEmail { get; set; }

    // Work Permit & Disability
    public bool HasWorkPermit { get; set; }
    public string? WorkPermitDetails { get; set; }
    public bool HasDisability { get; set; }
    public string? DisabilityDetails { get; set; }

    // Position Applied For
    public string? ReferenceNumber { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? AvailabilityNotice { get; set; }
    public DateTime? AvailabilityDate { get; set; }

    // Declarations & Background
    public bool HasCriminalRecord { get; set; }
    public string? CriminalRecordDetails { get; set; }
    public bool HasPendingCase { get; set; }
    public string? PendingCaseDetails { get; set; }
    public bool DismissedForMisconduct { get; set; }
    public string? DismissedDetails { get; set; }
    public bool PendingDisciplinaryCase { get; set; }
    public string? PendingDisciplinaryDetails { get; set; }
    public bool ResignedPendingDisciplinary { get; set; }
    public string? ResignedPendingDisciplinaryDetails { get; set; }
    public bool DischargedForIllHealth { get; set; }
    public string? DischargedDetails { get; set; }

    // Business Interests
    public bool BusinessWithState { get; set; }
    public string? BusinessDetails { get; set; }
    public bool WillRelinquishBusiness { get; set; }
    public string? WillRelinquishBusinessPlan { get; set; }

    // Experience & Professional Registration
    public int? PublicSectorYears { get; set; }
    public int? PrivateSectorYears { get; set; }
    public bool ReappointmentCondition { get; set; }
    public string? ReappointmentDepartment { get; set; }
    public string? ReappointmentConditionDetails { get; set; }
    public DateTime? ProfessionalRegistrationDate { get; set; }
    public string? ProfessionalInstitution { get; set; }
    public string? ProfessionalRegistrationNumber { get; set; }

    // Equity Information
    public bool EquityConsentGiven { get; set; }
    public string? Ethnicity { get; set; }
    public string? Gender { get; set; }
    public string? DisabilityStatus { get; set; }

    // Collections
    public List<LanguageProficiency> Languages { get; set; } = new();
    public List<QualificationRecord> Qualifications { get; set; } = new();
    public List<WorkExperienceRecord> WorkExperience { get; set; } = new();
    public List<ReferenceContact> References { get; set; } = new();
    public List<ScreeningAnswer> ScreeningAnswers { get; set; } = new();

    // Documents
    public DocumentInfo? CvDocument { get; set; }
    public DocumentInfo? IdDocument { get; set; }
    public DocumentInfo? QualificationDocument { get; set; }
    public DocumentInfo? DriversLicenseDocument { get; set; }
    public DocumentInfo? AdditionalDocument { get; set; }

    // Declaration
    public bool DeclarationAccepted { get; set; }
    public DateTime? DeclarationDate { get; set; }
    public string? SignatureData { get; set; }

    // Audit Trail
    public List<AuditEntry> AuditTrail { get; set; } = new();
}

public class DocumentInfo
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StorageToken { get; set; } = string.Empty;
    public DateTime? UploadedAtUtc { get; set; }
    public string? ParsedSummary { get; set; }
    public string DocumentType { get; set; } = string.Empty;
}
