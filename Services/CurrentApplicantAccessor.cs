using ERecruitment.Web.Models;
using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Services;

/// <summary>
/// Implementation of ICurrentApplicant that uses session storage with per-request caching.
/// Avoids multiple database hits within a single request.
/// </summary>
public class CurrentApplicantAccessor : ICurrentApplicant
{
    private const string ApplicantSessionKey = "ApplicantId";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRecruitmentRepository _repository;
    private Applicant? _cachedApplicant;
    private bool _hasAttemptedLoad;

    public CurrentApplicantAccessor(
        IHttpContextAccessor httpContextAccessor,
        IRecruitmentRepository repository)
    {
        _httpContextAccessor = httpContextAccessor;
        _repository = repository;
    }

    public async Task<Applicant?> GetAsync(CancellationToken cancellationToken = default)
    {
        // Return cached value if already loaded in this request
        if (_hasAttemptedLoad)
        {
            return _cachedApplicant;
        }

        var session = GetSession();
        if (session is null)
        {
            _hasAttemptedLoad = true;
            return null;
        }

        if (!session.TryGetValue(ApplicantSessionKey, out var bytes))
        {
            _hasAttemptedLoad = true;
            return null;
        }

        var id = new Guid(bytes);
        _cachedApplicant = await _repository.FindApplicantByIdAsync(id, cancellationToken);
        _hasAttemptedLoad = true;

        return _cachedApplicant;
    }

    public async Task SetAsync(Guid applicantId, CancellationToken cancellationToken = default)
    {
        var session = GetSession();
        if (session is null)
        {
            throw new InvalidOperationException("Session is not available.");
        }

        session.Set(ApplicantSessionKey, applicantId.ToByteArray());

        // Pre-load and cache the applicant for subsequent calls in this request
        _cachedApplicant = await _repository.FindApplicantByIdAsync(applicantId, cancellationToken);
        _hasAttemptedLoad = true;
    }

    public void Clear()
    {
        var session = GetSession();
        if (session is not null)
        {
            session.Remove(ApplicantSessionKey);
        }

        _cachedApplicant = null;
        _hasAttemptedLoad = false;
    }

    public Guid? GetApplicantId()
    {
        var session = GetSession();
        if (session is null)
        {
            return null;
        }

        if (!session.TryGetValue(ApplicantSessionKey, out var bytes))
        {
            return null;
        }

        return new Guid(bytes);
    }

    private ISession? GetSession()
    {
        return _httpContextAccessor.HttpContext?.Session;
    }
}
