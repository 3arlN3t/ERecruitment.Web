using ERecruitment.Web.Models;
using ERecruitment.Web.Utilities;
using ERecruitment.Web.Storage;
using ERecruitment.Web.Background;
using ERecruitment.Web.ViewModels;

namespace ERecruitment.Web.Services;

/// <summary>
/// Service responsible for applicant lifecycle: registration, authentication, and profile management.
/// Bounded context: Applicant aggregate root.
/// LOC Target: ~180 lines
/// </summary>
public class ApplicantManagementService : IApplicantManagementService
{
    private readonly IRecruitmentRepository _repository;
    private readonly ICvStorage _cvStorage;
    private readonly ICvParseJobQueue _jobQueue;

    public ApplicantManagementService(
        IRecruitmentRepository repository,
        ICvStorage cvStorage,
        ICvParseJobQueue jobQueue)
    {
        _repository = repository;
        _cvStorage = cvStorage;
        _jobQueue = jobQueue;
    }

    public async Task<RegistrationResult> RegisterApplicantAsync(
        RegisterViewModel model,
        CancellationToken cancellationToken = default)
    {
        var saIdProvided = !string.IsNullOrWhiteSpace(model.SaIdNumber);
        var passportProvided = !string.IsNullOrWhiteSpace(model.PassportNumber);

        if (!saIdProvided && !passportProvided)
        {
            return new RegistrationResult(false, "Please provide either a valid South African ID number or a passport number.");
        }

        if (saIdProvided && !SaIdValidator.IsValid(model.SaIdNumber!))
        {
            return new RegistrationResult(false, "The South African ID number entered is invalid.");
        }

        // Check for existing account
        var existing = await _repository.FindApplicantByEmailAsync(model.Email, cancellationToken);
        if (existing is not null)
        {
            return new RegistrationResult(false, "An account with this email already exists.");
        }

        // Create new applicant with transaction boundary
        var applicant = new Applicant
        {
            Email = model.Email.Trim(),
            PasswordHash = PasswordHasher.HashPassword(model.Password)
        };

        var profile = applicant.Profile;
        profile.FirstName = model.FirstName?.Trim();
        profile.LastName = model.LastName?.Trim();
        profile.SaIdNumber = model.SaIdNumber?.Trim();
        profile.PassportNumber = model.PassportNumber?.Trim();
        profile.PhoneNumber = model.PhoneNumber?.Trim();
        profile.Location = model.ResidentialAddress?.Trim();
        profile.ContactEmail = model.Email.Trim();

        applicant.EquityDeclaration = BuildEquityDeclaration(
            model.EquityConsent,
            model.EquityEthnicity,
            model.EquityGender,
            model.EquityDisability);

        await _repository.AddApplicantAsync(applicant, cancellationToken);

        // Audit trail
        await _repository.AddAuditEntryAsync(new AuditEntry
        {
            Actor = applicant.Email,
            Action = "Registered new applicant account"
        }, cancellationToken);

        return new RegistrationResult(true, null, applicant);
    }

    public async Task<AuthenticationResult> AuthenticateAsync(
        LoginViewModel model,
        CancellationToken cancellationToken = default)
    {
        var applicant = await _repository.FindApplicantByEmailAsync(model.Email.Trim(), cancellationToken);
        if (applicant is null)
        {
            return new AuthenticationResult(false, "Invalid email or password.");
        }

        if (!PasswordHasher.VerifyPassword(model.Password, applicant.PasswordHash))
        {
            return new AuthenticationResult(false, "Invalid email or password.");
        }

        return new AuthenticationResult(true, null, applicant);
    }

    public async Task<Applicant?> FindApplicantByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _repository.FindApplicantByEmailAsync(email, cancellationToken);
    }

    public async Task<Applicant?> GetApplicantAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.FindApplicantByIdAsync(id, cancellationToken);
    }

    public async Task<ProfileUpdateResult> UpdateProfileAsync(
        Applicant applicant,
        ProfileViewModel model,
        CancellationToken cancellationToken = default)
    {
        var profile = applicant.Profile;

        // Update all profile fields
        UpdateBasicInfo(profile, model);
        UpdateEmploymentInfo(profile, model);
        UpdateDeclarations(profile, model);
        UpdateCollections(profile, model);

        // Update equity declaration (for legacy support)
        applicant.EquityDeclaration = BuildEquityDeclaration(
            model.EquityConsent,
            model.EquityEthnicity,
            model.EquityGender,
            model.EquityDisability);

        // Handle document uploads with saga pattern (compensating transactions)
        await HandleDocumentUploadsAsync(applicant, model, cancellationToken);

        await _repository.UpdateApplicantAsync(applicant, cancellationToken);

        return new ProfileUpdateResult(true, null);
    }

    // ===== PRIVATE HELPER METHODS =====

    private static void UpdateBasicInfo(ApplicantProfile profile, ProfileViewModel model)
    {
        profile.FirstName = model.FirstName?.Trim();
        profile.LastName = model.LastName?.Trim();
        profile.DateOfBirth = model.DateOfBirth;
        profile.SaIdNumber = model.SaIdNumber?.Trim();
        profile.PassportNumber = model.PassportNumber?.Trim();
        profile.PhoneNumber = model.PhoneNumber?.Trim();
        profile.Location = model.Location?.Trim();
        profile.ContactEmail = model.ContactEmail?.Trim();
        profile.PreferredLanguage = model.PreferredLanguage;
    }

    private static void UpdateEmploymentInfo(ApplicantProfile profile, ProfileViewModel model)
    {
        profile.ReferenceNumber = model.ReferenceNumber?.Trim();
        profile.DepartmentName = model.DepartmentName?.Trim();
        profile.PositionName = model.PositionName?.Trim();
        profile.AvailabilityNotice = model.AvailabilityNotice;
        profile.AvailabilityDate = model.AvailabilityDate;
        profile.PublicSectorYears = model.PublicSectorYears;
        profile.PrivateSectorYears = model.PrivateSectorYears;
        profile.ProfessionalRegistrationDate = model.ProfessionalRegistrationDate;
        profile.ProfessionalInstitution = model.ProfessionalInstitution?.Trim();
        profile.ProfessionalRegistrationNumber = model.ProfessionalRegistrationNumber?.Trim();
    }

    private static void UpdateDeclarations(ApplicantProfile profile, ProfileViewModel model)
    {
        profile.IsSouthAfrican = model.IsSouthAfrican;
        profile.Nationality = model.Nationality?.Trim();
        profile.HasDisability = model.HasDisability;
        profile.HasWorkPermit = model.HasWorkPermit;
        profile.WorkPermitDetails = model.HasWorkPermit ? model.WorkPermitDetails?.Trim() : null;
        profile.HasCriminalRecord = model.HasCriminalRecord;
        profile.CriminalRecordDetails = model.HasCriminalRecord ? model.CriminalRecordDetails?.Trim() : null;
        profile.HasPendingCase = model.HasPendingCase;
        profile.PendingCaseDetails = model.HasPendingCase ? model.PendingCaseDetails?.Trim() : null;
        profile.DismissedForMisconduct = model.DismissedForMisconduct;
        profile.DismissedDetails = model.DismissedForMisconduct ? model.DismissedDetails?.Trim() : null;
        profile.PendingDisciplinaryCase = model.PendingDisciplinaryCase;
        profile.PendingDisciplinaryDetails = model.PendingDisciplinaryCase ? model.PendingDisciplinaryDetails?.Trim() : null;
        profile.ResignedPendingDisciplinary = model.ResignedPendingDisciplinary;
        profile.ResignedPendingDisciplinaryDetails = model.ResignedPendingDisciplinary ? model.ResignedPendingDisciplinaryDetails?.Trim() : null;
        profile.DischargedForIllHealth = model.DischargedForIllHealth;
        profile.DischargedDetails = model.DischargedForIllHealth ? model.DischargedDetails?.Trim() : null;
        profile.BusinessWithState = model.BusinessWithState;
        profile.BusinessDetails = model.BusinessWithState ? model.BusinessDetails?.Trim() : null;
        profile.WillRelinquishBusiness = model.WillRelinquishBusiness;
        profile.ReappointmentCondition = model.ReappointmentCondition;
        profile.ReappointmentDepartment = model.ReappointmentDepartment?.Trim();
        profile.ReappointmentConditionDetails = model.ReappointmentCondition ? model.ReappointmentConditionDetails?.Trim() : null;
        profile.DeclarationAccepted = model.DeclarationAccepted;
        profile.DeclarationDate = model.DeclarationAccepted ? (model.DeclarationDate ?? DateTime.UtcNow) : null;
        profile.SignatureData = model.DeclarationAccepted ? model.SignatureData : null;
    }

    private static void UpdateCollections(ApplicantProfile profile, ProfileViewModel model)
    {
        profile.Languages = model.Languages
            .Where(l => !string.IsNullOrWhiteSpace(l.LanguageName))
            .Select(l => new LanguageProficiency
            {
                LanguageName = l.LanguageName!.Trim(),
                SpeakProficiency = l.SpeakProficiency?.Trim() ?? string.Empty,
                ReadWriteProficiency = l.ReadWriteProficiency?.Trim() ?? string.Empty
            }).ToList();

        profile.Qualifications = model.Qualifications
            .Where(q => !string.IsNullOrWhiteSpace(q.QualificationName))
            .Select(q => new QualificationRecord
            {
                InstitutionName = q.InstitutionName?.Trim() ?? string.Empty,
                QualificationName = q.QualificationName!.Trim(),
                StudentNumber = q.StudentNumber?.Trim(),
                YearObtained = q.YearObtained?.Trim(),
                Status = q.Status
            }).ToList();

        profile.WorkExperience = model.WorkExperience
            .Where(w => !string.IsNullOrWhiteSpace(w.EmployerName) || !string.IsNullOrWhiteSpace(w.PositionHeld))
            .Select(w => new WorkExperienceRecord
            {
                EmployerName = w.EmployerName?.Trim() ?? string.Empty,
                PositionHeld = w.PositionHeld?.Trim() ?? string.Empty,
                FromDate = w.FromDate,
                ToDate = w.ToDate,
                Status = w.Status?.Trim() ?? string.Empty,
                ReasonForLeaving = w.ReasonForLeaving?.Trim()
            }).ToList();

        profile.References = model.References
            .Where(r => !string.IsNullOrWhiteSpace(r.Name))
            .Select(r => new ReferenceContact
            {
                Name = r.Name!.Trim(),
                Relationship = r.Relationship?.Trim() ?? string.Empty,
                ContactNumber = r.ContactNumber?.Trim() ?? string.Empty
            }).ToList();
    }

    private async Task HandleDocumentUploadsAsync(
        Applicant applicant,
        ProfileViewModel model,
        CancellationToken cancellationToken)
    {
        // CV upload with background parsing
        if (model.CvFile is not null)
        {
            var storageToken = await _cvStorage.SaveAsync(model.CvFile);
            applicant.Profile.Cv = new CvDocument
            {
                FileName = model.CvFile.FileName,
                ContentType = model.CvFile.ContentType,
                StorageToken = storageToken,
                ParsedSummary = null
            };

            await _repository.AddAuditEntryAsync(new AuditEntry
            {
                Actor = applicant.Email,
                Action = "Uploaded CV queued for parsing"
            }, cancellationToken);

            _jobQueue.Enqueue(new CvParseJob(storageToken, applicant.Id, model.CvFile.FileName));
        }

        // Other documents
        if (model.IdDocumentFile is not null)
        {
            applicant.Profile.IdDocument = await StoreDocumentAsync(model.IdDocumentFile, "ID/Passport");
        }

        if (model.QualificationDocumentFile is not null)
        {
            applicant.Profile.QualificationDocument = await StoreDocumentAsync(model.QualificationDocumentFile, "Qualification");
        }

        if (model.DriversLicenseDocumentFile is not null)
        {
            applicant.Profile.DriversLicenseDocument = await StoreDocumentAsync(model.DriversLicenseDocumentFile, "Drivers License");
        }

        if (model.AdditionalDocumentFile is not null)
        {
            applicant.Profile.AdditionalDocument = await StoreDocumentAsync(model.AdditionalDocumentFile, "Additional");
        }
    }

    private async Task<StoredDocument> StoreDocumentAsync(Microsoft.AspNetCore.Http.IFormFile file, string documentType)
    {
        var token = await _cvStorage.SaveAsync(file);
        return new StoredDocument
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            StorageToken = token,
            DocumentType = documentType,
            UploadedAtUtc = DateTime.UtcNow
        };
    }

    private static EquityDeclaration? BuildEquityDeclaration(
        bool consent,
        string? ethnicity,
        string? gender,
        string? disability)
    {
        if (!consent)
        {
            return new EquityDeclaration { ConsentGiven = false };
        }

        return new EquityDeclaration
        {
            ConsentGiven = true,
            Ethnicity = string.IsNullOrWhiteSpace(ethnicity) ? null : ethnicity.Trim(),
            Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim(),
            DisabilityStatus = string.IsNullOrWhiteSpace(disability) ? null : disability.Trim()
        };
    }
}

/// <summary>
/// Interface for applicant management operations.
/// </summary>
public interface IApplicantManagementService
{
    Task<RegistrationResult> RegisterApplicantAsync(RegisterViewModel model, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> AuthenticateAsync(LoginViewModel model, CancellationToken cancellationToken = default);
    Task<Applicant?> FindApplicantByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Applicant?> GetApplicantAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProfileUpdateResult> UpdateProfileAsync(Applicant applicant, ProfileViewModel model, CancellationToken cancellationToken = default);
}
