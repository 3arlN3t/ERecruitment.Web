using ERecruitment.Web.Services;
using ERecruitment.Web.ViewModels;
using ERecruitment.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERecruitment.Web.Controllers;

[Authorize]
public class ApplicationsController : Controller
{
    private readonly IApplicationWorkflowService _workflowService;
    private readonly ICurrentApplicant _currentApplicant;
    private readonly IRecruitmentRepository _repository;

    public ApplicationsController(
        IApplicationWorkflowService workflowService,
        ICurrentApplicant currentApplicant,
        IRecruitmentRepository repository)
    {
        _workflowService = workflowService;
        _currentApplicant = currentApplicant;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Job(Guid id)
    {
        var applicant = await _currentApplicant.GetAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var job = await _workflowService.GetJobPostingAsync(id);
        if (job is null)
        {
            return RedirectToAction("Dashboard", "Applicant");
        }

        var guard = EnsureProfileReadyForApplications(applicant);
        if (guard is not null)
        {
            return guard;
        }

        var flow = await _workflowService.StartApplicationAsync(applicant.Id, id);
        if (!flow.Success)
        {
            TempData["Flash"] = flow.ErrorMessage;
            return RedirectToAction("Dashboard", "Applicant");
        }

        var viewModel = new ApplicationStartViewModel
        {
            Job = job,
            ExistingApplication = flow.Application
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> KillerQuestion(Guid id, int index = 0)
    {
        var applicant = await _currentApplicant.GetAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var job = await _workflowService.GetJobPostingAsync(id);
        if (job is null)
        {
            return RedirectToAction("Dashboard", "Applicant");
        }

        var guard = EnsureProfileReadyForApplications(applicant);
        if (guard is not null)
        {
            return guard;
        }

        var questions = job.KillerQuestions;
        if (index < 0) index = 0;
        if (index >= questions.Count)
        {
            return RedirectToAction("Job", new { id });
        }
        var question = questions[index];
        if (question is null)
        {
            TempData["Flash"] = "This role does not have mandatory screening questions.";
            return RedirectToAction("Job", new { id });
        }

        var model = new KillerQuestionViewModel
        {
            JobId = id,
            Question = question,
            QuestionIndex = index,
            TotalQuestions = questions.Count
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KillerQuestion(KillerQuestionViewModel model)
    {
        var applicant = await _currentApplicant.GetAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var guardPost = EnsureProfileReadyForApplications(applicant);
        if (guardPost is not null)
        {
            return guardPost;
        }

        if (!ModelState.IsValid)
        {
            // Repopulate the Question field for the view
            var job = await _workflowService.GetJobPostingAsync(model.JobId);
            if (job is not null && model.QuestionIndex < job.KillerQuestions.Count)
            {
                model.Question = job.KillerQuestions[model.QuestionIndex];
            }
            return View(model);
        }

        var result = await _workflowService.SubmitKillerQuestionAsync(
            applicant.Id,
            applicant.Email,
            model.JobId,
            model.Answer,
            model.MeetsRequirement,
            model.QuestionIndex,
            model.SaveAsDraft);

        if (!result.Success)
        {
            TempData["Flash"] = result.ErrorMessage;
            return RedirectToAction("Job", new { id = model.JobId });
        }
        if (model.SaveAsDraft)
        {
            TempData["Flash"] = "Draft saved.";
            return RedirectToAction("KillerQuestion", new { id = model.JobId, index = model.QuestionIndex });
        }

        if (!result.QuestionPassed)
        {
            TempData["Flash"] = "Unfortunately you do not meet a mandatory requirement.";
            return RedirectToAction("Dashboard", "Applicant");
        }

        // Not a failure, so check if we continue or submit
        if (model.QuestionIndex + 1 < model.TotalQuestions)
        {
            // proceed to next question
            return RedirectToAction("KillerQuestion", new { id = model.JobId, index = model.QuestionIndex + 1 });
        }
        else
        {
            // last question was passed, submit the application automatically
            var submitResult = await _workflowService.SubmitScreenedApplicationAsync(applicant.Id, applicant.Email, model.JobId);
            if (submitResult.Success)
            {
                TempData["Flash"] = "Application submitted successfully! Our team will review and get back to you soon.";
            }
            else
            {
                TempData["Flash"] = submitResult.ErrorMessage ?? "An unexpected error occurred during submission.";
            }
            return RedirectToAction("Dashboard", "Applicant");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ScreeningFailed(Guid id)
    {
        var job = await _workflowService.GetJobPostingAsync(id);
        if (job is null)
        {
            return RedirectToAction("Dashboard", "Applicant");
        }
        return View(job);
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var job = await _workflowService.GetJobPostingAsync(id);
        if (job is null)
        {
            return RedirectToAction("Dashboard", "Applicant");
        }
        return View(job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitScreenedApplication(Guid id)
    {
        var applicant = await _currentApplicant.GetAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var guardPost = EnsureProfileReadyForApplications(applicant);
        if (guardPost is not null)
        {
            return guardPost;
        }

        var result = await _workflowService.SubmitScreenedApplicationAsync(applicant.Id, applicant.Email, id);
        if (result.Success)
        {
            TempData["Flash"] = "Application submitted successfully! Our team will review and get back to you soon.";
        }
        else
        {
            TempData["Flash"] = result.ErrorMessage ?? "An unexpected error occurred during submission.";
            return RedirectToAction("Job", new { id });
        }

        return RedirectToAction("Dashboard", "Applicant");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDirectApplication(Guid id)
    {
        var applicant = await _currentApplicant.GetAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var guardPost = EnsureProfileReadyForApplications(applicant);
        if (guardPost is not null)
        {
            return guardPost;
        }

        var result = await _workflowService.SubmitDirectApplicationAsync(applicant.Id, applicant.Email, id);
        if (result.Success)
        {
            TempData["Flash"] = "Application submitted successfully! Our team will review and get back to you soon.";
        }
        else
        {
            TempData["Flash"] = result.ErrorMessage;
        }

        return RedirectToAction("Dashboard", "Applicant");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(Guid id, string? reason)
    {
        var applicant = await _currentApplicant.GetAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }
        var result = await _workflowService.WithdrawApplicationAsync(applicant.Id, applicant.Email, id, reason);
        TempData["Flash"] = result.Success ? "Application withdrawn successfully." : result.ErrorMessage;
        TempData["FlashType"] = result.Success ? "success" : "error";
        return RedirectToAction("Dashboard", "Applicant");
    }

    [HttpGet]
    public async Task<IActionResult> Timeline(Guid id)
    {
        var applicant = await _currentApplicant.GetAsync();
        if (applicant is null)
        {
            return RedirectToAction("Login", "Account");
        }
        var application = await _repository.FindJobApplicationAsync(applicant.Id, id);
        var job = await _workflowService.GetJobPostingAsync(id);
        if (application is null || job is null)
        {
            return RedirectToAction("Dashboard", "Applicant");
        }
        var vm = new ApplicationTimelineViewModel
        {
            Application = application,
            Job = job,
            Events = application.AuditTrail.OrderByDescending(e => e.TimestampUtc).ToList()
        };
        return View(vm);
}

    private IActionResult? EnsureProfileReadyForApplications(Models.Applicant applicant)
    {
        var profile = applicant.Profile ?? new Models.ApplicantProfile();
        if (profile.MeetsMinimumRequirements())
        {
            return null;
        }

        var missingFields = profile.GetMissingCriticalFields();
        var missingDescription = missingFields.Any()
            ? string.Join(", ", missingFields)
            : "the required profile information";

        TempData["Flash"] = $"Please complete your profile before applying. Missing information: {missingDescription}.";
        TempData["FlashType"] = "warning";
        return RedirectToAction("Profile", "Applicant");
    }
}
