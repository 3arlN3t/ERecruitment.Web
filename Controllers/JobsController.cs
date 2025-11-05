using ERecruitment.Web.Models;
using ERecruitment.Web.Services;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERecruitment.Web.Controllers;

[Authorize(Roles = "Admin")]
public class JobsController : Controller
{
    private readonly IRecruitmentRepository _repo;
    private readonly ICurrentApplicant _currentApplicant;
    private readonly IApplicationWorkflowService _workflowService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IRecruitmentRepository repo,
        ICurrentApplicant currentApplicant,
        IApplicationWorkflowService workflowService,
        ILogger<JobsController> logger)
    {
        _repo = repo;
        _currentApplicant = currentApplicant;
        _workflowService = workflowService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var jobs = await _repo.GetAllJobPostingsAsync();
        var isAdmin = User?.IsInRole("Admin") == true;
        var isApplicant = User?.Identity?.IsAuthenticated == true && !isAdmin;

        _logger.LogInformation("Is Applicant: {isApplicant}", isApplicant);

        var viewModel = new BrowseJobsViewModel
        {
            Jobs = jobs,
            IsAdmin = isAdmin,
            IsApplicant = isApplicant
        };

        // If user is an authenticated applicant, fetch their applications
        if (isApplicant)
        {
            var applicant = await _currentApplicant.GetAsync();
            if (applicant is not null)
            {
                _logger.LogInformation("Applicant ID: {applicantId}", applicant.Id);
                viewModel.ApplicantApplications = await _workflowService.GetApplicantApplicationsAsync(applicant.Id);
                _logger.LogInformation("Applicant has {count} applications", viewModel.ApplicantApplications.Count);
                foreach (var app in viewModel.ApplicantApplications)
                {
                    _logger.LogInformation("Application for Job ID: {jobId}", app.JobPostingId);
                }
            }
            else
            {
                _logger.LogWarning("Applicant is null");
            }
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var job = await _repo.GetJobPostingAsync(id);
        if (job is null) return NotFound();
        return View(job);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateJobPostingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateJobPostingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var job = new JobPosting
        {
            // Primary identification
            PostNumber = model.PostNumber,
            ReferenceNumber = model.ReferenceNumber,
            Title = model.Title,
            Department = model.Department ?? string.Empty,
            
            // Salary section
            SalaryRange = model.SalaryRange,
            
            // Centre section
            Centre = model.Centre,
            Province = model.Province,
            Location = model.Centre, // Map Centre to Location for backward compatibility
            
            // Requirements section
            Requirements = model.Requirements,
            
            // Duties section
            DutiesDescription = model.DutiesDescription,
            
            // Enquiries section
            EnquiriesContactPerson = model.EnquiriesContactPerson,
            EnquiriesPhone = model.EnquiriesPhone,
            
            // Applications section
            ApplicationEmail = model.ApplicationEmail,
            ClosingDate = model.ClosingDate,
            
            // Notes section
            AdditionalNotes = model.AdditionalNotes,
            
            // Legacy fields
            Description = GenerateDescription(model), // Generate from Requirements + Duties
            KillerQuestions = SplitLines(model.KillerQuestionsText),
            
            // Metadata
            DatePosted = DateTime.UtcNow,
            DateLastModified = DateTime.UtcNow,
            IsActive = true,
            PostedByUserId = User.Identity?.Name
        };
        
        await _repo.AddJobPostingAsync(job);
        TempData["Flash"] = "Government-compliant job posting created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var job = await _repo.GetJobPostingAsync(id);
        if (job is null) return NotFound();
        
        var vm = new CreateJobPostingViewModel
        {
            // Primary identification
            PostNumber = job.PostNumber,
            ReferenceNumber = job.ReferenceNumber,
            Title = job.Title,
            Department = job.Department,
            
            // Salary section
            SalaryRange = job.SalaryRange,
            
            // Centre section
            Centre = job.Centre,
            Province = job.Province,
            
            // Requirements section
            Requirements = job.Requirements,
            
            // Duties section
            DutiesDescription = job.DutiesDescription,
            
            // Enquiries section
            EnquiriesContactPerson = job.EnquiriesContactPerson,
            EnquiriesPhone = job.EnquiriesPhone,
            
            // Applications section
            ApplicationEmail = job.ApplicationEmail,
            ClosingDate = job.ClosingDate ?? DateTime.UtcNow.AddDays(30),
            
            // Notes section
            AdditionalNotes = job.AdditionalNotes,
            
            // Legacy fields
            KillerQuestionsText = string.Join(Environment.NewLine, job.KillerQuestions)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateJobPostingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var job = await _repo.GetJobPostingAsync(id);
        if (job is null) return NotFound();
        
        // Update all fields
        job.PostNumber = model.PostNumber;
        job.ReferenceNumber = model.ReferenceNumber;
        job.Title = model.Title;
        job.Department = model.Department ?? string.Empty;
        
        job.SalaryRange = model.SalaryRange;
        
        job.Centre = model.Centre;
        job.Province = model.Province;
        job.Location = model.Centre; // Map Centre to Location for backward compatibility
        
        job.Requirements = model.Requirements;
        
        job.DutiesDescription = model.DutiesDescription;
        
        job.EnquiriesContactPerson = model.EnquiriesContactPerson;
        job.EnquiriesPhone = model.EnquiriesPhone;
        
        job.ApplicationEmail = model.ApplicationEmail;
        job.ClosingDate = model.ClosingDate;
        
        job.AdditionalNotes = model.AdditionalNotes;
        
        job.Description = GenerateDescription(model); // Regenerate description
        job.KillerQuestions = SplitLines(model.KillerQuestionsText);
        job.DateLastModified = DateTime.UtcNow;
        
        await _repo.UpdateJobPostingAsync(job);
        TempData["Flash"] = "Government-compliant job posting updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var job = await _repo.GetJobPostingAsync(id);
        if (job is null) return NotFound();
        return View(job);
    }

    [HttpGet]
    public async Task<IActionResult> Applications(Guid id, int page = 1, int pageSize = 25)
    {
        var job = await _repo.GetJobPostingAsync(id);
        if (job is null) return NotFound();

        var paged = await _repo.GetJobApplicationsPagedAsync(page, pageSize, jobId: id);
        var vm = new JobApplicationsViewModel
        {
            JobPosting = job,
            Applications = paged.Items,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _repo.DeleteJobPostingAsync(id);
        TempData["Flash"] = "Job deleted.";
        return RedirectToAction(nameof(Index));
    }

    private static List<string> SplitLines(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? new List<string>()
            : text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static string GenerateDescription(CreateJobPostingViewModel model)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(model.Requirements))
            parts.Add($"REQUIREMENTS:\n{model.Requirements}");
        
        if (!string.IsNullOrWhiteSpace(model.DutiesDescription))
            parts.Add($"\nDUTIES:\n{model.DutiesDescription}");
        
        return string.Join("\n\n", parts);
    }
}


