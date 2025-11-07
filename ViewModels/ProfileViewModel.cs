using System;
using System.Collections.Generic;
using ERecruitment.Web.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

public class ProfileViewModel
{
    // Section A – Advertised Post
    [Display(Name = "Reference number (as stated in the advert)")]
    public string? ReferenceNumber { get; set; }

    [Display(Name = "Department where the position was advertised")]
    public string? DepartmentName { get; set; }

    [Display(Name = "Position Name")]
    public string? PositionName { get; set; }

    [Display(Name = "Availability")]
    public string? AvailabilityNotice { get; set; }

    [Display(Name = "Availability Date")]
    [DataType(DataType.Date)]
    public DateTime? AvailabilityDate { get; set; }

    public List<JobPostingOption> OpenJobPostings { get; set; } = new();

    // Personal Information
    [Display(Name = "Surname")]
    public string? LastName { get; set; }

    [Display(Name = "Full names")]
    public string? FirstName { get; set; }

    [Display(Name = "Date Of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Identity Number")]
    public string? SaIdNumber { get; set; }

    [Display(Name = "Passport number")]
    public string? PassportNumber { get; set; }

    [Display(Name = "Ethnicity")]
    public string? EquityEthnicity { get; set; }

    [Display(Name = "Gender")]
    public string? EquityGender { get; set; }

    [Display(Name = "Do you have a disability?")]
    public bool HasDisability { get; set; }

    [Display(Name = "Disability details")]
    public string? DisabilityDetails { get; set; }

    [Display(Name = "Are you a South African citizen?")]
    public bool IsSouthAfrican { get; set; } = true;

    [Display(Name = "Nationality")]
    public string? Nationality { get; set; }

    [Display(Name = "Do you have a valid work permit?")]
    public bool HasWorkPermit { get; set; }

    [Display(Name = "Work permit details")]
    public string? WorkPermitDetails { get; set; }

    [Display(Name = "Have you been convicted or found guilty of a criminal offence (including an admission of guilt)?")]
    public bool HasCriminalRecord { get; set; }

    [Display(Name = "Criminal offence details")]
    public string? CriminalRecordDetails { get; set; }

    [Display(Name = "Do you have any pending criminal case against you?")]
    public bool HasPendingCase { get; set; }

    [Display(Name = "Pending case details")]
    public string? PendingCaseDetails { get; set; }

    [Display(Name = "Have you ever been dismissed for misconduct from the Public Service?")]
    public bool DismissedForMisconduct { get; set; }

    [Display(Name = "Dismissal details")]
    public string? DismissedDetails { get; set; }

    [Display(Name = "Do you have any pending disciplinary case against you?")]
    public bool PendingDisciplinaryCase { get; set; }

    [Display(Name = "Pending disciplinary details")]
    public string? PendingDisciplinaryDetails { get; set; }

    [Display(Name = "Have you resigned pending any disciplinary proceeding?")]
    public bool ResignedPendingDisciplinary { get; set; }

    [Display(Name = "Resignation details")]
    public string? ResignedPendingDisciplinaryDetails { get; set; }

    [Display(Name = "Have you been discharged or retired on grounds of ill-health?")]
    public bool DischargedForIllHealth { get; set; }

    [Display(Name = "Discharge details")]
    public string? DischargedDetails { get; set; }

    [Display(Name = "Are you conducting business with the State or a director of such a company?")]
    public bool BusinessWithState { get; set; }

    [Display(Name = "Business details")]
    public string? BusinessDetails { get; set; }

    [Display(Name = "If employed, will you relinquish such business interests?")]
    public bool WillRelinquishBusiness { get; set; }

    [Display(Name = "Relinquishment plan")]
    public string? WillRelinquishBusinessPlan { get; set; }

    [Display(Name = "Public sector experience (years)")]
    public int? PublicSectorYears { get; set; }

    [Display(Name = "Private sector experience (years)")]
    public int? PrivateSectorYears { get; set; }

    [Display(Name = "Any condition preventing re-appointment?")]
    public bool ReappointmentCondition { get; set; }

    [Display(Name = "Department")]
    public string? ReappointmentDepartment { get; set; }

    [Display(Name = "Condition details")]
    public string? ReappointmentConditionDetails { get; set; }

    [Display(Name = "If profession requires registration, provide details")]
    public DateTime? ProfessionalRegistrationDate { get; set; }
    public string? ProfessionalInstitution { get; set; }
    public string? ProfessionalRegistrationNumber { get; set; }

    // Contact details
    [Display(Name = "Preferred language for correspondence")]
    public string? PreferredLanguage { get; set; }

    [Display(Name = "Email")]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Display(Name = "Contact Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Physical Address")]
    public string? Location { get; set; }

    // Language proficiency
    public List<LanguageProficiencyInput> Languages { get; set; } = new();

    // Qualifications
    public List<QualificationInput> Qualifications { get; set; } = new();

    // Work experience
    public List<WorkExperienceInput> WorkExperience { get; set; } = new();

    // References
    public List<ReferenceInput> References { get; set; } = new();

    // Declaration
    [Display(Name = "I declare that all information provided is complete and correct.")]
    public bool DeclarationAccepted { get; set; }

    [Display(Name = "Declaration date")]
    [DataType(DataType.Date)]
    public DateTime? DeclarationDate { get; set; }

    [Display(Name = "Signature")]
    public string? SignatureData { get; set; }

    // Documents
    public IFormFile? CvFile { get; set; }
    public IFormFile? IdDocumentFile { get; set; }
    public IFormFile? QualificationDocumentFile { get; set; }
    public IFormFile? DriversLicenseDocumentFile { get; set; }
    public IFormFile? AdditionalDocumentFile { get; set; }

    // Existing documents for display
    public CvDocument? ExistingCv { get; set; }
    public StoredDocument? ExistingIdDocument { get; set; }
    public StoredDocument? ExistingQualificationDocument { get; set; }
    public StoredDocument? ExistingDriversLicenseDocument { get; set; }
    public StoredDocument? ExistingAdditionalDocument { get; set; }

    // Legacy support
    public bool EquityConsent { get; set; }
    public string? EquityDisability { get; set; }
}

public class JobPostingOption
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool ReferenceGenerated { get; set; }
}

public class LanguageProficiencyInput
{
    public string? LanguageName { get; set; }
    public string? SpeakProficiency { get; set; }
    public string? ReadWriteProficiency { get; set; }
}

public class QualificationInput
{
    public string? InstitutionName { get; set; }
    public string? QualificationName { get; set; }
    public string? StudentNumber { get; set; }
    public string? YearObtained { get; set; }
    public string Status { get; set; } = "Completed";
}

public class WorkExperienceInput
{
    public string? EmployerName { get; set; }
    public string? PositionHeld { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
    public string? ReasonForLeaving { get; set; }
}

public class ReferenceInput
{
    public string? Name { get; set; }
    public string? Relationship { get; set; }
    public string? ContactNumber { get; set; }
}
