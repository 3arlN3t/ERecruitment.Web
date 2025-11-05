using ERecruitment.Web.Models;
using ERecruitment.Web.Notifications;

namespace ERecruitment.Web.Services;

/// <summary>
/// Service responsible for job application workflow and state transitions.
/// Bounded context: Application aggregate root and recruitment business rules.
/// LOC Target: ~150 lines
/// </summary>
public class ApplicationWorkflowService : IApplicationWorkflowService
{
    private readonly IRecruitmentRepository _repository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;

    public ApplicationWorkflowService(
        IRecruitmentRepository repository,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer)
    {
        _repository = repository;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
    }

    public async Task<ApplicationFlowResult> StartApplicationAsync(
        Guid applicantId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetJobPostingAsync(jobId, cancellationToken);
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.");
        }

        // Check if job has expired
        if (job.IsExpired)
        {
            return new ApplicationFlowResult(false, $"This position closed on {job.ClosingDate:dd MMM yyyy}. Applications are no longer accepted.");
        }

        // Check if job is inactive
        if (!job.IsActive)
        {
            return new ApplicationFlowResult(false, "This position is no longer accepting applications.");
        }

        // Check if application already exists
        var existing = await _repository.FindJobApplicationAsync(applicantId, jobId, cancellationToken);
        if (existing is not null)
        {
            return new ApplicationFlowResult(true, null, existing);
        }

        // Create new draft application
        var application = new JobApplication
        {
            ApplicantId = applicantId,
            JobPostingId = jobId,
            JobTitle = job.Title,
            Status = ApplicationStatus.Draft
        };

        await _repository.AddJobApplicationAsync(application, cancellationToken);

        return new ApplicationFlowResult(true, null, application);
    }

    public async Task<ApplicationFlowResult> SubmitDirectApplicationAsync(
        Guid applicantId,
        string applicantEmail,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetJobPostingAsync(jobId, cancellationToken);
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.");
        }

        // Check if job has expired
        if (job.IsExpired)
        {
            return new ApplicationFlowResult(false, $"This position closed on {job.ClosingDate:dd MMM yyyy}. Applications are no longer accepted.");
        }

        // Check if job is inactive
        if (!job.IsActive)
        {
            return new ApplicationFlowResult(false, "This position is no longer accepting applications.");
        }

        // Direct submission not allowed if job has killer questions
        if (job.KillerQuestions.Any())
        {
            return new ApplicationFlowResult(false, "This position requires answering screening questions.");
        }

        var application = await _repository.FindJobApplicationAsync(applicantId, jobId, cancellationToken);
        if (application is null)
        {
            var startResult = await StartApplicationAsync(applicantId, jobId, cancellationToken);
            if (!startResult.Success || startResult.Application is null)
            {
                return new ApplicationFlowResult(false, startResult.ErrorMessage ?? "Unable to create application.");
            }
            application = startResult.Application;
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

        if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Submitted))
        {
            return new ApplicationFlowResult(false, "Your application is no longer in a state that can be submitted.");
        }

        if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Submitted))
        {
            return new ApplicationFlowResult(false, "Your application is no longer in a state that can be submitted.");
        }

        // Submit with transaction boundary
        application.Status = ApplicationStatus.Submitted;
        application.SubmittedAtUtc = DateTime.UtcNow;
        application.AuditTrail.Add(new AuditEntry
        {
            Actor = applicantEmail,
            Action = "Submitted application via direct submission (no screening questions)",
            JobApplicationId = application.Id
        });

        await _repository.UpdateJobApplicationAsync(application, cancellationToken);

        // Send confirmation email (fire-and-forget is OK for notifications)
        var html = await _templateRenderer.RenderAsync("ApplicationReceived", new { JobTitle = application.JobTitle });
        _ = _emailSender.SendAsync(applicantEmail, "Application received", html);

        return new ApplicationFlowResult(true, null, application);
    }

    public async Task<ApplicationFlowResult> SubmitKillerQuestionAsync(
        Guid applicantId,
        string applicantEmail,
        Guid jobId,
        string answer,
        bool meetsRequirement,
        int questionIndex,
        bool saveAsDraft,
        CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetJobPostingAsync(jobId, cancellationToken);
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.", null, meetsRequirement);
        }

        // Check if job has expired
        if (job.IsExpired)
        {
            return new ApplicationFlowResult(false, $"This position closed on {job.ClosingDate:dd MMM yyyy}. Applications are no longer accepted.");
        }

        // Check if job is inactive
        if (!job.IsActive)
        {
            return new ApplicationFlowResult(false, "This position is no longer accepting applications.");
        }

        var application = await _repository.FindJobApplicationAsync(applicantId, jobId, cancellationToken);
        if (application is null)
        {
            var startResult = await StartApplicationAsync(applicantId, jobId, cancellationToken);
            if (!startResult.Success || startResult.Application is null)
            {
                return new ApplicationFlowResult(false, "Application not found. Please start the application process again.", null, meetsRequirement);
            }
            application = startResult.Application;
        }

        // Validate question index
        if (questionIndex >= job.KillerQuestions.Count)
        {
            return new ApplicationFlowResult(false, "Invalid question index.", application, false);
        }

        // Save answer
        var answerRecord = application.ScreeningAnswers.FirstOrDefault(a => a.Order == questionIndex);
        if (answerRecord is null)
        {
            answerRecord = new ScreeningAnswer
            {
                Order = questionIndex,
                Question = job.KillerQuestions[questionIndex]
            };
            application.ScreeningAnswers.Add(answerRecord);
        }
        answerRecord.Answer = answer;
        answerRecord.MeetsRequirement = meetsRequirement;

        // Handle draft vs final answer
        if (saveAsDraft)
        {
            if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Draft))
            {
                return new ApplicationFlowResult(false, "Application state no longer allows saving progress.");
            }

            application.Status = ApplicationStatus.Draft;
            application.AuditTrail.Add(new AuditEntry
            {
                Actor = applicantEmail,
                Action = $"Saved draft for question {questionIndex + 1}",
                JobApplicationId = application.Id
            });
            await _repository.UpdateJobApplicationAsync(application, cancellationToken);
            return new ApplicationFlowResult(true, null, application, meetsRequirement);
        }

        // Failed killer question - auto-reject
        if (!meetsRequirement)
        {
            if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Rejected))
            {
                return new ApplicationFlowResult(false, "Application cannot be auto-rejected from the current state.");
            }

            application.Status = ApplicationStatus.Rejected;
            application.RejectionReason = "Did not meet mandatory requirement";
            application.AuditTrail.Add(new AuditEntry
            {
                Actor = "system",
                Action = "Application auto-rejected after failing killer question",
                JobApplicationId = application.Id
            });

            await _repository.UpdateJobApplicationAsync(application, cancellationToken);

            var html = await _templateRenderer.RenderAsync("ApplicationRejected",
                new { JobTitle = application.JobTitle, PersonalisedReason = application.RejectionReason });
            _ = _emailSender.SendAsync(applicantEmail, $"Application update: {application.JobTitle}", html);

            return new ApplicationFlowResult(true, null, application, false);
        }

        // Passed - record it
        application.AuditTrail.Add(new AuditEntry
        {
            Actor = applicantEmail,
            Action = $"Answered question {questionIndex + 1}",
            JobApplicationId = application.Id
        });

        await _repository.UpdateJobApplicationAsync(application, cancellationToken);

        return new ApplicationFlowResult(true, null, application, true);
    }

    public async Task<ApplicationFlowResult> SubmitScreenedApplicationAsync(
        Guid applicantId,
        string applicantEmail,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var application = await _repository.FindJobApplicationAsync(applicantId, jobId, cancellationToken);
        if (application is null)
        {
            return new ApplicationFlowResult(false, "Application not found.");
        }

        var job = await _repository.GetJobPostingAsync(jobId, cancellationToken);
        if (job is null)
        {
            return new ApplicationFlowResult(false, "Job not found.");
        }

        // Check if job has expired
        if (job.IsExpired)
        {
            return new ApplicationFlowResult(false, $"This position closed on {job.ClosingDate:dd MMM yyyy}. Applications are no longer accepted.");
        }

        // Check if job is inactive
        if (!job.IsActive)
        {
            return new ApplicationFlowResult(false, "This position is no longer accepting applications.");
        }

        // Validate all questions answered and passed
        if (application.ScreeningAnswers.Count != job.KillerQuestions.Count ||
            application.ScreeningAnswers.Any(a => a.MeetsRequirement == false))
        {
            return new ApplicationFlowResult(false, "Cannot submit: screening questions incomplete or failed.");
        }

        // Submit with transaction boundary
        application.Status = ApplicationStatus.Submitted;
        application.SubmittedAtUtc = DateTime.UtcNow;
        application.RejectionReason = null;
        application.AuditTrail.Add(new AuditEntry
        {
            Actor = applicantEmail,
            Action = "Submitted application after passing all killer questions",
            JobApplicationId = application.Id
        });

        await _repository.UpdateJobApplicationAsync(application, cancellationToken);

        var html = await _templateRenderer.RenderAsync("ApplicationReceived", new { JobTitle = application.JobTitle });
        _ = _emailSender.SendAsync(applicantEmail, "Application received", html);

        return new ApplicationFlowResult(true, null, application);
    }

    public async Task<ApplicationFlowResult> WithdrawApplicationAsync(
        Guid applicantId,
        string applicantEmail,
        Guid jobId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        // Reload the application fresh from the database to avoid stale entity issues
        var application = await _repository.FindJobApplicationAsync(applicantId, jobId, cancellationToken);
        if (application is null)
        {
            return new ApplicationFlowResult(false, "Application not found.");
        }

        if (application.Status == ApplicationStatus.Withdrawn)
        {
            return new ApplicationFlowResult(true, null, application);
        }

        if (!ApplicationStatusRules.CanTransition(application.Status, ApplicationStatus.Withdrawn))
        {
            return new ApplicationFlowResult(false, "This application can no longer be withdrawn.");
        }

        // Withdraw with transaction boundary
        application.Status = ApplicationStatus.Withdrawn;
        
        // Create a fresh audit entry
        var withdrawalEntry = new AuditEntry
        {
            Actor = applicantEmail,
            Action = $"Application withdrawn{(string.IsNullOrWhiteSpace(reason) ? string.Empty : $": {reason}")}",
            JobApplicationId = application.Id,
            TimestampUtc = DateTime.UtcNow
        };
        
        application.AuditTrail.Add(withdrawalEntry);

        await _repository.UpdateJobApplicationAsync(application, cancellationToken);

        // Notify applicant
        var html = await _templateRenderer.RenderAsync("ApplicationWithdrawn",
            new { JobTitle = application.JobTitle, Reason = reason ?? string.Empty });
        _ = _emailSender.SendAsync(applicantEmail, $"Application withdrawn: {application.JobTitle}", html);

        return new ApplicationFlowResult(true, null, application);
    }

    public async Task<IReadOnlyCollection<JobApplication>> GetApplicantApplicationsAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetApplicantApplicationsAsync(applicantId, cancellationToken);
    }

    public async Task<JobPosting?> GetJobPostingAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetJobPostingAsync(jobId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<JobPosting>> GetAllJobPostingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllJobPostingsAsync(cancellationToken);
    }
}

/// <summary>
/// Interface for application workflow operations.
/// </summary>
public interface IApplicationWorkflowService
{
    Task<ApplicationFlowResult> StartApplicationAsync(Guid applicantId, Guid jobId, CancellationToken cancellationToken = default);
    Task<ApplicationFlowResult> SubmitDirectApplicationAsync(Guid applicantId, string applicantEmail, Guid jobId, CancellationToken cancellationToken = default);
    Task<ApplicationFlowResult> SubmitKillerQuestionAsync(Guid applicantId, string applicantEmail, Guid jobId, string answer, bool meetsRequirement, int questionIndex, bool saveAsDraft, CancellationToken cancellationToken = default);
    Task<ApplicationFlowResult> SubmitScreenedApplicationAsync(Guid applicantId, string applicantEmail, Guid jobId, CancellationToken cancellationToken = default);
    Task<ApplicationFlowResult> WithdrawApplicationAsync(Guid applicantId, string applicantEmail, Guid jobId, string? reason, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<JobApplication>> GetApplicantApplicationsAsync(Guid applicantId, CancellationToken cancellationToken = default);
    Task<JobPosting?> GetJobPostingAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<JobPosting>> GetAllJobPostingsAsync(CancellationToken cancellationToken = default);
}
