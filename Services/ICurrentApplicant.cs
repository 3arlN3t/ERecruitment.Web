using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

/// <summary>
/// Abstracts access to the currently authenticated applicant from the session.
/// Decouples services from HttpContext and session management.
/// </summary>
public interface ICurrentApplicant
{
    /// <summary>
    /// Gets the currently authenticated applicant from session.
    /// Returns null if no applicant is logged in.
    /// </summary>
    Task<Applicant?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the current applicant in session after successful login/registration.
    /// </summary>
    Task SetAsync(Guid applicantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the current applicant from session (logout).
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the applicant ID from session without loading the full entity.
    /// Returns null if no applicant is logged in.
    /// </summary>
    Guid? GetApplicantId();
}
