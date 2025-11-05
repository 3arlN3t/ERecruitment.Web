namespace ERecruitment.Web.Models;

public class ApplicantProfile
{
    // Section A – Advertised Post
    public string? ReferenceNumber { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? AvailabilityNotice { get; set; } // e.g. Immediate, 1 Week, etc.
    public DateTime? AvailabilityDate { get; set; }

    // Personal information
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? SaIdNumber { get; set; }
    public string? PassportNumber { get; set; }
    public bool HasDisability { get; set; }
    public string? DisabilityDetails { get; set; }
    public bool IsSouthAfrican { get; set; } = true;
    public string? Nationality { get; set; }
    public bool HasWorkPermit { get; set; }
    public string? WorkPermitDetails { get; set; }
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
    public bool BusinessWithState { get; set; }
    public string? BusinessDetails { get; set; }
    public bool WillRelinquishBusiness { get; set; }
    public int? PublicSectorYears { get; set; }
    public int? PrivateSectorYears { get; set; }
    public bool ReappointmentCondition { get; set; }
    public string? ReappointmentDepartment { get; set; }
    public string? ReappointmentConditionDetails { get; set; }
    public DateTime? ProfessionalRegistrationDate { get; set; }
    public string? ProfessionalInstitution { get; set; }
    public string? ProfessionalRegistrationNumber { get; set; }

    public string? PreferredLanguage { get; set; }
    public string? ContactEmail { get; set; }

    // Collections
    public CvDocument? Cv { get; set; }
    public StoredDocument? IdDocument { get; set; }
    public StoredDocument? QualificationDocument { get; set; }
    public StoredDocument? DriversLicenseDocument { get; set; }
    public StoredDocument? AdditionalDocument { get; set; }

    public List<LanguageProficiency> Languages { get; set; } = new();
    public List<QualificationRecord> Qualifications { get; set; } = new();
    public List<WorkExperienceRecord> WorkExperience { get; set; } = new();
    public List<ReferenceContact> References { get; set; } = new();

    // Declaration
    public bool DeclarationAccepted { get; set; }
    public DateTime? DeclarationDate { get; set; }
    public string? SignatureData { get; set; }
}
