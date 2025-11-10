using System;
using System.Collections.Generic;
using System.Linq;
using ERecruitment.Web.Models;
using ERecruitment.Web.Services;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERecruitment.Web.Controllers;

[Authorize]
public class ApplicantController : Controller
{
    private readonly IApplicantManagementService _applicantService;
    private readonly IApplicationWorkflowService _workflowService;
    private readonly ICurrentApplicant _currentApplicant;
    private readonly ILogger<ApplicantController> _logger;

    public ApplicantController(
        IApplicantManagementService applicantService,
        IApplicationWorkflowService workflowService,
        ICurrentApplicant currentApplicant,
        ILogger<ApplicantController> logger)
    {
        _applicantService = applicantService;
        _workflowService = workflowService;
        _currentApplicant = currentApplicant;
        _logger = logger;
    }

    public async Task<IActionResult> Dashboard()
    {
        var applicant = await RequireApplicantAsync();
        if (applicant is null)
        {
            _logger.LogWarning("Applicant not found, redirecting to login.");
            return RedirectToAction("Login", "Account");
        }
        
        _logger.LogInformation("Dashboard loaded for Applicant ID: {ApplicantId}", applicant.Id);

        var allJobs = await _workflowService.GetAllJobPostingsAsync();
        var allApplications = await _workflowService.GetApplicantApplicationsAsync(applicant.Id);
        
        _logger.LogInformation("Found {ApplicationCount} total applications for Applicant ID: {ApplicantId}", allApplications.Count, applicant.Id);
        foreach (var app in allApplications)
        {
            _logger.LogInformation("  - Application for Job ID: {JobId}, Status: {Status}", app.JobPostingId, app.Status);
        }

        // Filter to only show open jobs and applications to open jobs
        var openJobs = allJobs.Where(j => j.IsAcceptingApplications).ToList();
        var applicationsForOpenJobs = allApplications
            .Where(app => allJobs.Any(j => j.Id == app.JobPostingId && j.IsAcceptingApplications))
            .ToList();
        
        _logger.LogInformation("Found {ApplicationCount} applications for open jobs.", applicationsForOpenJobs.Count);

        var viewModel = new ApplicantDashboardViewModel
        {
            Applicant = applicant,
            Jobs = openJobs,
            Applications = applicationsForOpenJobs
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var applicant = await RequireApplicantAsync();
            if (applicant is null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = BuildProfileViewModel(applicant);
            await PopulateOpenJobOptionsAsync(model);
            return View("Profile2", model);
        }
        catch (Exception ex)
        {
            // Log the error details
            Console.WriteLine($"Error in Profile GET: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw; // Re-throw to see in browser
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var applicant = await RequireApplicantAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }

        EnsureCollectionDefaults(model);
        if (!ModelState.IsValid)
        {
            PopulateExistingDocuments(model, applicant);
            await PopulateOpenJobOptionsAsync(model);
            return View("Profile2", model);
        }

        var result = await _applicantService.UpdateProfileAsync(applicant, model);
        if (!result.Success)
        {
            PopulateExistingDocuments(model, applicant);
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to save profile.");
            await PopulateOpenJobOptionsAsync(model);
            return View("Profile2", model);
        }

        if (model.CvFile is not null)
        {
            TempData["Flash"] = applicant.Profile.Cv?.ParsedSuccessfully == true
                ? "Profile updated. We parsed your CV and highlighted new skills."
                : "Profile updated. Your CV requires manual review but is queued for recruiters.";
        }
        else
        {
            TempData["Flash"] = "Profile updated.";
        }
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Profile2()
    {
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Profile2(ProfileViewModel model)
    {
        return RedirectToAction(nameof(Profile));
    }

    private async Task<Models.Applicant?> RequireApplicantAsync()
    {
        return await _currentApplicant.GetAsync();
    }

    private ProfileViewModel BuildProfileViewModel(Models.Applicant applicant)
    {
        try
        {
            var profile = applicant?.Profile ?? new Models.ApplicantProfile();
            
            Console.WriteLine($"Building profile for applicant: {applicant?.Email}");
            Console.WriteLine($"Profile is null: {applicant?.Profile == null}");
            
            var model = new ProfileViewModel
            {
                ReferenceNumber = profile.ReferenceNumber,
                DepartmentName = profile.DepartmentName,
                PositionName = profile.PositionName,
                AvailabilityNotice = profile.AvailabilityNotice,
                AvailabilityDate = profile.AvailabilityDate,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                DateOfBirth = profile.DateOfBirth,
                SaIdNumber = profile.SaIdNumber,
                PassportNumber = profile.PassportNumber,
                PhoneNumber = profile.PhoneNumber,
                Location = profile.Location,
                ContactEmail = profile.ContactEmail ?? applicant?.Email,
                HasDisability = profile.HasDisability,
                IsSouthAfrican = profile.IsSouthAfrican,
                Nationality = profile.Nationality,
                HasWorkPermit = profile.HasWorkPermit,
                WorkPermitDetails = profile.WorkPermitDetails,
                HasCriminalRecord = profile.HasCriminalRecord,
                CriminalRecordDetails = profile.CriminalRecordDetails,
                HasPendingCase = profile.HasPendingCase,
                PendingCaseDetails = profile.PendingCaseDetails,
                DismissedForMisconduct = profile.DismissedForMisconduct,
                DismissedDetails = profile.DismissedDetails,
                PendingDisciplinaryCase = profile.PendingDisciplinaryCase,
                PendingDisciplinaryDetails = profile.PendingDisciplinaryDetails,
                ResignedPendingDisciplinary = profile.ResignedPendingDisciplinary,
                ResignedPendingDisciplinaryDetails = profile.ResignedPendingDisciplinaryDetails,
                DischargedForIllHealth = profile.DischargedForIllHealth,
                DischargedDetails = profile.DischargedDetails,
                BusinessWithState = profile.BusinessWithState,
                BusinessDetails = profile.BusinessDetails,
                WillRelinquishBusiness = profile.WillRelinquishBusiness,
                PublicSectorYears = profile.PublicSectorYears,
                PrivateSectorYears = profile.PrivateSectorYears,
                ReappointmentCondition = profile.ReappointmentCondition,
                ReappointmentDepartment = profile.ReappointmentDepartment,
                ReappointmentConditionDetails = profile.ReappointmentConditionDetails,
                ProfessionalRegistrationDate = profile.ProfessionalRegistrationDate,
                ProfessionalInstitution = profile.ProfessionalInstitution,
                ProfessionalRegistrationNumber = profile.ProfessionalRegistrationNumber,
                PreferredLanguage = profile.PreferredLanguage,
                DeclarationAccepted = profile.DeclarationAccepted,
                DeclarationDate = profile.DeclarationDate,
                SignatureData = profile.SignatureData,
                EquityConsent = applicant?.EquityDeclaration?.ConsentGiven ?? false,
                EquityEthnicity = applicant?.EquityDeclaration?.Ethnicity,
                EquityGender = applicant?.EquityDeclaration?.Gender,
                EquityDisability = applicant?.EquityDeclaration?.DisabilityStatus,
                ExistingCv = profile.Cv,
                ExistingIdDocument = profile.IdDocument,
                ExistingQualificationDocument = profile.QualificationDocument,
                ExistingDriversLicenseDocument = profile.DriversLicenseDocument,
                ExistingAdditionalDocument = profile.AdditionalDocument
            };
            
            Console.WriteLine("Mapping collections...");
            
            model.Languages = profile.Languages?.Select(l => new LanguageProficiencyInput
            {
                LanguageName = l.LanguageName,
                SpeakProficiency = l.SpeakProficiency,
                ReadWriteProficiency = l.ReadWriteProficiency
            }).ToList() ?? new List<LanguageProficiencyInput>();

            model.Qualifications = profile.Qualifications?.Select(q => new QualificationInput
            {
                InstitutionName = q.InstitutionName,
                QualificationName = q.QualificationName,
                StudentNumber = q.StudentNumber,
                YearObtained = q.YearObtained,
                Status = q.Status
            }).ToList() ?? new List<QualificationInput>();

            model.WorkExperience = profile.WorkExperience?.Select(w => new WorkExperienceInput
            {
                EmployerName = w.EmployerName,
                PositionHeld = w.PositionHeld,
                FromDate = w.FromDate,
                ToDate = w.ToDate,
                Status = w.Status,
                ReasonForLeaving = w.ReasonForLeaving
            }).ToList() ?? new List<WorkExperienceInput>();

            model.References = profile.References?.Select(r => new ReferenceInput
            {
                Name = r.Name,
                Relationship = r.Relationship,
                ContactNumber = r.ContactNumber
            }).ToList() ?? new List<ReferenceInput>();

            Console.WriteLine("Ensuring collection defaults...");
            EnsureCollectionDefaults(model);
            
            Console.WriteLine("Profile ViewModel created successfully");
            return model;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in BuildProfileViewModel: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    private void EnsureCollectionDefaults(ProfileViewModel model)
    {
        model.Languages ??= new List<LanguageProficiencyInput>();
        model.Qualifications ??= new List<QualificationInput>();
        model.WorkExperience ??= new List<WorkExperienceInput>();
        model.References ??= new List<ReferenceInput>();

        if (model.Languages.Count == 0) model.Languages.Add(new LanguageProficiencyInput());
        if (model.Qualifications.Count == 0) model.Qualifications.Add(new QualificationInput());
        if (model.WorkExperience.Count == 0) model.WorkExperience.Add(new WorkExperienceInput());
        if (model.References.Count == 0) model.References.Add(new ReferenceInput());
    }

    private void PopulateExistingDocuments(ProfileViewModel model, Models.Applicant applicant)
    {
        model.ExistingCv = applicant.Profile.Cv;
       model.ExistingIdDocument = applicant.Profile.IdDocument;
       model.ExistingQualificationDocument = applicant.Profile.QualificationDocument;
       model.ExistingDriversLicenseDocument = applicant.Profile.DriversLicenseDocument;
       model.ExistingAdditionalDocument = applicant.Profile.AdditionalDocument;
    }

    private async Task PopulateOpenJobOptionsAsync(ProfileViewModel model)
    {
        var openJobs = await _workflowService.GetAllJobPostingsAsync();
        var referenceTracker = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var options = new List<JobPostingOption>();

        foreach (var job in openJobs.Where(j => j.IsAcceptingApplications))
        {
            var (reference, generated) = ResolveReference(job, referenceTracker);
            if (string.IsNullOrWhiteSpace(reference))
            {
                continue;
            }

            options.Add(new JobPostingOption
            {
                Id = job.Id,
                ReferenceNumber = reference,
                Title = job.Title ?? string.Empty,
                Department = job.Department ?? string.Empty,
                ReferenceGenerated = generated
            });
        }

        model.OpenJobPostings = options
            .OrderBy(option => option.ReferenceNumber, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static (string Reference, bool Generated) ResolveReference(JobPosting job, HashSet<string> referenceTracker)
    {
        if (job is null)
        {
            return (string.Empty, false);
        }

        var baseReference = string.IsNullOrWhiteSpace(job.ReferenceNumber)
            ? BuildTestingReference(job)
            : job.ReferenceNumber.Trim();

        if (string.IsNullOrWhiteSpace(baseReference))
        {
            return (string.Empty, false);
        }

        var uniqueReference = baseReference;
        var counter = 1;
        while (!referenceTracker.Add(uniqueReference))
        {
            counter++;
            uniqueReference = $"{baseReference}-{counter}";
        }

        return (uniqueReference, string.IsNullOrWhiteSpace(job.ReferenceNumber));
    }

    private static string BuildTestingReference(JobPosting job)
    {
        if (!string.IsNullOrWhiteSpace(job.PostNumber))
        {
            return $"TEST-{Sanitize(job.PostNumber)}";
        }

        if (!string.IsNullOrWhiteSpace(job.Title))
        {
            var words = job.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var acronym = string.Concat(words.Take(3).Select(w => char.ToUpperInvariant(w[0])));
            if (!string.IsNullOrWhiteSpace(acronym))
            {
                return $"TEST-{acronym}-{job.Id.ToString("N")[..4].ToUpperInvariant()}";
            }
        }

        return $"TEST-{job.Id.ToString("N")[..6].ToUpperInvariant()}";
    }

    private static string Sanitize(string value)
    {
        var filtered = value.Where(char.IsLetterOrDigit).ToArray();
        if (filtered.Length == 0)
        {
            return "REF";
        }

        return new string(filtered).ToUpperInvariant();
    }

    [HttpGet]
    public async Task<IActionResult> ExportCv()
    {
        var applicant = await RequireApplicantAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = BuildProfileViewModel(applicant);
        return View(model);
    }
}
