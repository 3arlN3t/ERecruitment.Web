using ERecruitment.Web.Models;
using ERecruitment.Web.Services;
using ERecruitment.Web.Storage;
using ERecruitment.Web.Security;

namespace ERecruitment.Web.Background;

public class CvParseWorker : BackgroundService
{
    private readonly ILogger<CvParseWorker> _logger;
    private readonly IServiceProvider _services;
    private readonly ICvParseJobQueue _queue;
    private readonly ICvStorage _storage;
    private readonly IFileScanner _scanner;

    public CvParseWorker(ILogger<CvParseWorker> logger, IServiceProvider services, ICvParseJobQueue queue, ICvStorage storage, IFileScanner scanner)
    {
        _logger = logger;
        _services = services;
        _queue = queue;
        _storage = storage;
        _scanner = scanner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_queue.TryDequeue(out var job) && job is not null)
                {
                    await ProcessJobAsync(job, stoppingToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CV parse worker");
            }
        }
    }

    private async Task ProcessJobAsync(CvParseJob job, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRecruitmentRepository>();
        var applicant = await repo.FindApplicantByIdAsync(job.ApplicantId, ct);
        if (applicant is null)
        {
            return;
        }

        // AV scan stub
        var safe = await AntivirusScanStubAsync(job.StorageToken, ct);
        if (!safe)
        {
            var audit = new AuditEntry { Actor = "system", Action = "CV failed AV scan" };
            await repo.AddAuditEntryAsync(audit, ct);
            return;
        }

        await using var stream = await _storage.OpenReadAsync(job.StorageToken, ct);
        if (!await _scanner.IsSafeAsync(stream, ct))
        {
            await repo.AddAuditEntryAsync(new AuditEntry { Actor = "system", Action = "CV failed background scan" }, ct);
            return;
        }
        using var reader = new StreamReader(stream);
        var summary = $"Parsed {job.FileName} successfully and extracted key skills.";

        if (applicant.Profile.Cv is not null && applicant.Profile.Cv.StorageToken == job.StorageToken)
        {
            applicant.Profile.Cv.ParsedSummary = summary;
        }
        await repo.UpdateApplicantAsync(applicant, ct);

        await repo.AddAuditEntryAsync(new AuditEntry { Actor = "system", Action = "CV parsed in background" }, ct);
    }

    private Task<bool> AntivirusScanStubAsync(string storageToken, CancellationToken ct)
    {
        // Placeholder for AV scanning integration
        return Task.FromResult(true);
    }
}


