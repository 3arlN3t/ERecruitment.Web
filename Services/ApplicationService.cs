using System.IO;
using System.Linq;
using ERecruitment.Web.Models;
using ERecruitment.Web.Utilities;
using ERecruitment.Web.Storage;
using ERecruitment.Web.Background;
using ERecruitment.Web.Notifications;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Services;

public class ApplicationService : IApplicationService
{
    private const string ApplicantSessionKey = "ApplicantId";
    private const string AdminSessionKey = "AdminAccess";
    private readonly IRecruitmentRepository _repository;
    private readonly ICvStorage _cvStorage;
    private readonly ICvParseJobQueue _jobQueue;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;

    public ApplicationService(IRecruitmentRepository repository, ICvStorage cvStorage, ICvParseJobQueue jobQueue, IEmailSender emailSender, IEmailTemplateRenderer templateRenderer)
    {
        _repository = repository;
        _cvStorage = cvStorage;
        _jobQueue = jobQueue;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
    }

    public RegistrationResult RegisterApplicant(RegisterViewModel model)
    {
        if (!SaIdValidator.IsValid(model.SaIdNumber))
        {
            return new RegistrationResult(false, "The South African ID number entered is invalid.");
        }

        if (_repository.FindApplicantByEmailAsync(model.Email).GetAwaiter().GetResult() is not null)
        {
            return new RegistrationResult(false, "An account with this email already exists.");
        }

        var applicant = new Applicant
        {
            Email = model.Email.Trim(),
            PasswordHash = PasswordHasher.HashPassword(model.Password)
        };

        applicant.Profile.SaIdNumber = model.SaIdNumber;
        applicant.EquityDeclaration = BuildEquityDeclaration(applicant.EquityDeclaration, model.EquityConsent, model.EquityEthnicity, model.EquityGender, model.EquityDisability);

        _repository.AddApplicantAsync(applicant).GetAwaiter().GetResult();
        _repository.AddAuditEntryAsync(new AuditEntry
        {
            Actor = applicant.Email,
            Action = "Registered new account with validated SA ID"
        }).GetAwaiter().GetResult();

        return new RegistrationResult(true, null, applicant);
    }

    public AuthenticationResult Authenticate(LoginViewModel model)
    {
        var applicant = _repository.FindApplicantByEmailAsync(model.Email.Trim()).GetAwaiter().GetResult();
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

    public Applicant? FindApplicantByEmail(string email) => _repository.FindApplicantByEmailAsync(email).GetAwaiter().GetResult();

    public Applicant? GetApplicant(Guid id) => _repository.FindApplicantByIdAsync(id).GetAwaiter().GetResult();

    public Applicant? GetApplicantForSession(ISession session)
    {
        if (session.TryGetValue(ApplicantSessionKey, out var bytes))
        {
            var id = new Guid(bytes);
            return GetApplicant(id);
        }

        return null;
    }

    public void SetApplicantSession(ISession session, Guid applicantId)
    {
        session.Set(ApplicantSessionKey, applicantId.ToByteArray());
    }

    public void ClearApplicantSession(ISession session)
    {
        session.Remove(ApplicantSessionKey);
        session.Remove(AdminSessionKey);
    }

    public async Task<ProfileUpdateResult> UpdateProfileAsync(Applicant applicant, ProfileViewModel model)
    {
        var profile = applicant.Profile;

        profile.ReferenceNumber = model.ReferenceNumber?.Trim();
        profile.DepartmentName = model.DepartmentName?.Trim();
        profile.PositionName = model.PositionName?.Trim();
        profile.AvailabilityNotice = model.AvailabilityNotice;
        profile.AvailabilityDate = model.AvailabilityDate;

        profile.FirstName = model.FirstName?.Trim();
        profile.LastName = model.LastName?.Trim();
        profile.DateOfBirth = model.DateOfBirth;
        profile.SaIdNumber = model.SaIdNumber?.Trim();
        profile.PassportNumber = model.PassportNumber?.Trim();
        profile.PhoneNumber = model.PhoneNumber?.Trim();
        profile.Location = model.Location?.Trim();
        profile.ContactEmail = model.ContactEmail?.Trim();

        profile.HasDisability = model.HasDisability;
        profile.IsSouthAfrican = model.IsSouthAfrican;
        profile.Nationality = model.Nationality?.Trim();
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
        profile.PublicSectorYears = model.PublicSectorYears;
        profile.PrivateSectorYears = model.PrivateSectorYears;
        profile.ReappointmentCondition = model.ReappointmentCondition;
        profile.ReappointmentDepartment = model.ReappointmentDepartment?.Trim();
        profile.ReappointmentConditionDetails = model.ReappointmentCondition ? model.ReappointmentConditionDetails?.Trim() : null;
        profile.ProfessionalRegistrationDate = model.ProfessionalRegistrationDate;
        profile.ProfessionalInstitution = model.ProfessionalInstitution?.Trim();
        profile.ProfessionalRegistrationNumber = model.ProfessionalRegistrationNumber?.Trim();
        profile.PreferredLanguage = model.PreferredLanguage;

        profile.DeclarationAccepted = model.DeclarationAccepted;
        profile.DeclarationDate = model.DeclarationAccepted ? (model.DeclarationDate ?? DateTime.UtcNow) : null;
        profile.SignatureData = model.DeclarationAccepted ? model.SignatureData : null;

        // Update languages
        profile.Languages = model.Languages
            .Where(l => !string.IsNullOrWhiteSpace(l.LanguageName))
            .Select(l => new LanguageProficiency
            {
                LanguageName = l.LanguageName!.Trim(),
                SpeakProficiency = l.SpeakProficiency?.Trim() ?? string.Empty,
                ReadWriteProficiency = l.ReadWriteProficiency?.Trim() ?? string.Empty
            }).ToList();

        // Update qualifications
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

        // Update work experience
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

        // References
        profile.References = model.References
            .Where(r => !string.IsNullOrWhiteSpace(r.Name))
            .Select(r => new ReferenceContact
            {
                Name = r.Name!.Trim(),
                Relationship = r.Relationship?.Trim() ?? string.Empty,
                ContactNumber = r.ContactNumber?.Trim() ?? string.Empty
            }).ToList();

        // Employment equity declaration (legacy support)
        applicant.EquityDeclaration = BuildEquityDeclaration(applicant.EquityDeclaration, model.EquityConsent, model.EquityEthnicity, model.EquityGender, model.EquityDisability);

        // Document uploads
        if (model.CvFile is not null)
        {
            var storageToken = await _cvStorage.SaveAsync(model.CvFile);
            profile.Cv = new CvDocument
            {
                FileName = model.CvFile.FileName,
                ContentType = model.CvFile.ContentType,
                StorageToken = storageToken,
                ParsedSummary = null
            };

            _repository.AddAuditEntryAsync(new AuditEntry
            {
                Actor = applicant.Email,
                Action = "Uploaded CV queued for parsing"
            }).GetAwaiter().GetResult();

            _jobQueue.Enqueue(new CvParseJob(storageToken, applicant.Id, model.CvFile.FileName));
        }

        if (model.IdDocumentFile is not null)
        {
            profile.IdDocument = await StoreDocumentAsync(model.IdDocumentFile, "ID/Passport");
        }

        if (model.QualificationDocumentFile is not null)
        {
            profile.QualificationDocument = await StoreDocumentAsync(model.QualificationDocumentFile, "Qualification");
        }

        if (model.DriversLicenseDocumentFile is not null)
        {
            profile.DriversLicenseDocument = await StoreDocumentAsync(model.DriversLicenseDocumentFile, "Drivers License");
        }

        if (model.AdditionalDocumentFile is not null)
        {
            profile.AdditionalDocument = await StoreDocumentAsync(model.AdditionalDocumentFile, "Additional");
        }

        await _repository.UpdateApplicantAsync(applicant);
        return new ProfileUpdateResult(true, null);
    }

    public JobPosting? GetJobPosting(Guid id) => _repository.GetJobPostingAsync(id).GetAwaiter().GetResult();

    public IReadOnlyCollection<JobPosting> GetJobs() => _repository.GetAllJobPostingsAsync().GetAwaiter().GetResult();

    public ApplicationFlowResult StartApplication(Applicant applicant, Guid jobId)
    {
        var job = _repository.GetJobPostingAsync(jobId).GetAwaiter().GetResult();
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.");
        }

        var application = applicant.Applications.FirstOrDefault(a => a.JobPostingId == jobId)
            ?? _repository.FindJobApplicationAsync(applicant.Id, jobId).GetAwaiter().GetResult();

        if (application is null)
        {
            application = new JobApplication
            {
                ApplicantId = applicant.Id,
                JobPostingId = jobId,
                JobTitle = job.Title,
                Status = ApplicationStatus.Draft
            };

            _repository.AddJobApplicationAsync(application).GetAwaiter().GetResult();
        }

        if (!applicant.Applications.Any(a => a.Id == application.Id))
        {
            applicant.Applications.Add(application);
        }

        return new ApplicationFlowResult(true, null, application);
    }

    public async Task<ApplicationFlowResult> SubmitDirectApplication(Applicant applicant, Guid jobId)
    {
        var job = await _repository.GetJobPostingAsync(jobId);
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.");
        }

        // Check if job has killer questions - should not use direct submission if it does
        if (job.KillerQuestions.Any())
        {
            return new ApplicationFlowResult(false, "This position requires answering screening questions. Please use the screening process.");
        }

        var application = await _repository.FindJobApplicationAsync(applicant.Id, jobId);
        if (application is null)
        {
            var startResult = StartApplication(applicant, jobId);
            if (!startResult.Success || startResult.Application is null)
            {
                return new ApplicationFlowResult(false, startResult.ErrorMessage ?? "Unable to create application.");
            }

            application = startResult.Application;
        }

        if (!applicant.Applications.Any(a => a.Id == application.Id))
        {
            applicant.Applications.Add(application);
        }

        // Check if already submitted
        if (application.Status == ApplicationStatus.Submitted)
        {
            return new ApplicationFlowResult(false, "You have already submitted an application for this position.");
        }

        if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Submitted))
        {
            return new ApplicationFlowResult(false, "Your application is no longer in a state that can be submitted.");
        }

        // Submit the application
        if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Submitted))
        {
            return new ApplicationFlowResult(false, "Your application is no longer in a state that can be submitted.");
        }

        application.Status = ApplicationStatus.Submitted;
        application.SubmittedAtUtc = DateTime.UtcNow;
        var auditEntry = new AuditEntry
        {
            Actor = applicant.Email,
            Action = "Submitted application via direct submission (no screening questions)",
            JobApplicationId = application.Id
        };
        application.AuditTrail.Add(auditEntry);

        await _repository.UpdateJobApplicationAsync(application);

        // Send confirmation email
        var html = await _templateRenderer.RenderAsync("ApplicationReceived", new { JobTitle = application.JobTitle });
        _ = _emailSender.SendAsync(applicant.Email, "Application received", html);

        return new ApplicationFlowResult(true, null, application);
    }

    public async Task<ApplicationFlowResult> SubmitKillerQuestion(Applicant applicant, Guid jobId, string answer, bool pass, int questionIndex, bool saveAsDraft)
    {
        var job = await _repository.GetJobPostingAsync(jobId);
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.", null, pass);
        }

        var application = await _repository.FindJobApplicationAsync(applicant.Id, jobId);
        if (application is null)
        {
            // Try to create the application if it doesn't exist (fallback)
            var startResult = StartApplication(applicant, jobId);
            if (!startResult.Success || startResult.Application is null)
            {
                return new ApplicationFlowResult(false, "Application not found. Please start the application process again.", null, pass);
            }
            application = startResult.Application;
        }

        if (!applicant.Applications.Any(a => a.Id == application.Id))
        {
            applicant.Applications.Add(application);
        }

        // Save answer into structured list (refactored logic)
        var answerRecord = application.ScreeningAnswers.FirstOrDefault(a => a.Order == questionIndex);
        if (answerRecord is null)
        {
            // Ensure we don't go out of bounds
            if (questionIndex >= job.KillerQuestions.Count)
            {
                return new ApplicationFlowResult(false, "Invalid question index.", application, false);
            }
            answerRecord = new ScreeningAnswer { Order = questionIndex, Question = job.KillerQuestions[questionIndex] };
            application.ScreeningAnswers.Add(answerRecord);
        }
        answerRecord.Answer = answer;
        answerRecord.MeetsRequirement = pass;

        if (saveAsDraft)
        {
            if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Draft))
            {
                return new ApplicationFlowResult(false, "Application state no longer allows saving progress.", application, pass);
            }

            application.Status = ApplicationStatus.Draft;
            application.AuditTrail.Add(new AuditEntry
            {
                Actor = applicant.Email,
                Action = $"Saved draft for question {questionIndex + 1}",
                JobApplicationId = application.Id
            });
            await _repository.UpdateJobApplicationAsync(application);
            return new ApplicationFlowResult(true, null, application, pass);
        }

        if (!pass)
        {
            if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Rejected))
            {
                return new ApplicationFlowResult(false, "Application cannot be auto-rejected from the current state.", application, pass);
            }

            application.Status = ApplicationStatus.Rejected;
            application.RejectionReason = "Did not meet mandatory requirement";
            application.AuditTrail.Add(new AuditEntry
            {
                Actor = "system",
                Action = "Application auto-rejected after failing killer question",
                JobApplicationId = application.Id
            });

            var html = await _templateRenderer.RenderAsync("ApplicationRejected", new { JobTitle = application.JobTitle, PersonalisedReason = application.RejectionReason });
            _ = _emailSender.SendAsync(applicant.Email, $"Application update: {application.JobTitle}", html);
        }
        else
        {
             application.AuditTrail.Add(new AuditEntry
            {
                Actor = applicant.Email,
                Action = $"Answered question {questionIndex + 1}",
                JobApplicationId = application.Id
            });
        }
        
        await _repository.UpdateJobApplicationAsync(application);
        return new ApplicationFlowResult(true, null, application, pass);
    }

    public async Task<ApplicationFlowResult> SubmitScreenedApplication(Applicant applicant, Guid jobId)
    {
        var application = await _repository.FindJobApplicationAsync(applicant.Id, jobId);
        if (application is null)
        {
            return new ApplicationFlowResult(false, "Application not found.");
        }

        // Double-check that all questions have been answered and passed
        var job = await _repository.GetJobPostingAsync(jobId);
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.");
        }

        if (application.ScreeningAnswers.Count != job.KillerQuestions.Count || application.ScreeningAnswers.Any(a => a.MeetsRequirement == false))
        {
            return new ApplicationFlowResult(false, "Cannot submit application: screening questions are incomplete or have failed.");
        }

        application.Status = ApplicationStatus.Submitted;
        application.SubmittedAtUtc = DateTime.UtcNow;
        application.RejectionReason = null;
        application.AuditTrail.Add(new AuditEntry
        {
            Actor = applicant.Email,
            Action = "Submitted application after passing all killer questions and final review",
            JobApplicationId = application.Id
        });

        var successHtml = await _templateRenderer.RenderAsync("ApplicationReceived", new { JobTitle = application.JobTitle });
        _ = _emailSender.SendAsync(applicant.Email, "Application received", successHtml);

        await _repository.UpdateJobApplicationAsync(application);
        return new ApplicationFlowResult(true, null, application);
    }

    public ApplicationFlowResult WithdrawApplication(Applicant applicant, Guid jobId, string? reason)
    {
        var application = _repository.FindJobApplicationAsync(applicant.Id, jobId).GetAwaiter().GetResult();
        if (application is null)
        {
            return new ApplicationFlowResult(false, "Application not found.");
        }
        if (application.Status == ApplicationStatus.Withdrawn)
        {
            return new ApplicationFlowResult(true, null, application);
        }
        if (!applicant.Applications.Any(a => a.Id == application.Id))
        {
            applicant.Applications.Add(application);
        }
        if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Withdrawn))
        {
            return new ApplicationFlowResult(false, "This application can no longer be withdrawn.");
        }

        application.Status = ApplicationStatus.Withdrawn;
        application.AuditTrail.Add(new AuditEntry
        {
            Actor = applicant.Email,
            Action = $"Application withdrawn{(string.IsNullOrWhiteSpace(reason) ? string.Empty : $": {reason}") }",
            JobApplicationId = application.Id
        });
        _repository.UpdateJobApplicationAsync(application).GetAwaiter().GetResult();
        // Notify applicant
        var html = _templateRenderer.RenderAsync("ApplicationWithdrawn", new { JobTitle = application.JobTitle, Reason = reason ?? string.Empty }).GetAwaiter().GetResult();
        _ = _emailSender.SendAsync(applicant.Email, $"Application withdrawn: {application.JobTitle}", html);
        return new ApplicationFlowResult(true, null, application);
    }

    public IReadOnlyCollection<JobApplication> GetApplications(Applicant applicant) => applicant.Applications.ToList();

    public BulkRejectResult BulkRejectApplications(AdminBulkRejectViewModel model)
    {
        var updated = 0;
        var entries = new List<AuditEntry>();

        // Get all applicants by iterating through pages
        var allApplicants = new List<Applicant>();
        int page = 1;
        const int pageSize = 100;
        PagedResult<Applicant> pageResult;
        do
        {
            pageResult = _repository.GetApplicantsPagedAsync(page, pageSize).GetAwaiter().GetResult();
            allApplicants.AddRange(pageResult.Items);
            page++;
        } while (pageResult.Items.Count == pageSize);

        foreach (var applicant in allApplicants)
        {
            foreach (var application in applicant.Applications.Where(a => model.SelectedApplicationIds.Contains(a.Id)))
            {
                var personalised = PersonaliseTemplate(model.TemplateBody, applicant, application);
                if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Rejected))
                {
                    continue;
                }

                application.Status = ApplicationStatus.Rejected;
                application.RejectionReason = personalised;
                application.AuditTrail.Add(new AuditEntry
                {
                    Actor = "admin",
                    Action = "Bulk rejection issued",
                    JobApplicationId = application.Id
                });

                var auditEntry = new AuditEntry
                {
                    Actor = "admin",
                    Action = $"Bulk rejected application {application.Id} using template",
                    JobApplicationId = application.Id
                };
                entries.Add(auditEntry);
                _repository.AddAuditEntryAsync(auditEntry).GetAwaiter().GetResult();
                updated++;

                // Send email notification
                _ = _emailSender.SendAsync(applicant.Email, $"Application update: {application.JobTitle}", personalised);

                _repository.UpdateJobApplicationAsync(application).GetAwaiter().GetResult();
            }
        }

        return new BulkRejectResult(updated, entries);
    }

    public AdminDashboardModel BuildAdminDashboard()
    {
        // Get all applicants by iterating through pages
        var allApplicants = new List<Applicant>();
        int page = 1;
        const int pageSize = 100;
        PagedResult<Applicant> pageResult;
        do
        {
            pageResult = _repository.GetApplicantsPagedAsync(page, pageSize).GetAwaiter().GetResult();
            allApplicants.AddRange(pageResult.Items);
            page++;
        } while (pageResult.Items.Count == pageSize);

        var applications = allApplicants.SelectMany(a => a.Applications).ToList();

        var equitySummary = allApplicants
            .Select(a => a.EquityDeclaration)
            .Where(e => e?.ConsentGiven == true && !string.IsNullOrWhiteSpace(e.Ethnicity))
            .GroupBy(e => e!.Ethnicity!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Get all audit log entries by iterating through pages
        var allAuditEntries = new List<AuditEntry>();
        page = 1;
        PagedResult<AuditEntry> auditPageResult;
        do
        {
            auditPageResult = _repository.GetAuditLogPagedAsync(page, pageSize).GetAwaiter().GetResult();
            allAuditEntries.AddRange(auditPageResult.Items);
            page++;
        } while (auditPageResult.Items.Count == pageSize);

        // Get all email deliveries by iterating through pages
        var allEmailDeliveries = new List<EmailDelivery>();
        page = 1;
        PagedResult<EmailDelivery> emailPageResult;
        do
        {
            emailPageResult = _repository.GetEmailDeliveriesPagedAsync(page, pageSize).GetAwaiter().GetResult();
            allEmailDeliveries.AddRange(emailPageResult.Items);
            page++;
        } while (emailPageResult.Items.Count == pageSize);

        return new AdminDashboardModel(
            TotalApplicants: allApplicants.Count,
            TotalApplications: applications.Count,
            ActiveApplications: applications.Count(a => a.Status is ApplicationStatus.Submitted or ApplicationStatus.Interview or ApplicationStatus.Offer),
            Jobs: GetJobs(),
            Applications: applications,
            EquityOptInSummary: equitySummary,
            AuditLog: allAuditEntries,
            EmailDeliveries: allEmailDeliveries);
    }

    private static string PersonaliseTemplate(string template, Applicant applicant, JobApplication application)
    {
        var firstName = applicant.Profile.FirstName;
        if (string.IsNullOrWhiteSpace(firstName))
        {
            firstName = applicant.Email.Split('@').First();
        }

        return template
            .Replace("{{firstName}}", firstName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{jobTitle}}", application.JobTitle, StringComparison.OrdinalIgnoreCase);
    }

    private static EquityDeclaration BuildEquityDeclaration(EquityDeclaration? existing, bool consent, string? ethnicity, string? gender, string? disability)
    {
        var declaration = existing ?? new EquityDeclaration();

        var sanitizedEthnicity = string.IsNullOrWhiteSpace(ethnicity) ? null : ethnicity.Trim();
        var sanitizedGender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();
        var sanitizedDisability = string.IsNullOrWhiteSpace(disability) ? null : disability.Trim();

        var effectiveConsent = consent
            || declaration.ConsentGiven
            || sanitizedEthnicity is not null
            || sanitizedGender is not null
            || sanitizedDisability is not null;

        declaration.ConsentGiven = effectiveConsent;
        declaration.Ethnicity = sanitizedEthnicity;
        declaration.Gender = sanitizedGender;
        declaration.DisabilityStatus = sanitizedDisability;

        return declaration;
    }

    private async Task<StoredDocument> StoreDocumentAsync(IFormFile file, string documentType)
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
}
