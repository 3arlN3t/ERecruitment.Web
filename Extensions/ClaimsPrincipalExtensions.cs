using System.Security.Claims;

namespace ERecruitment.Web.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to easily access custom claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    private const string ApplicantIdClaimType = "ApplicantId";

    /// <summary>
    /// Gets the ApplicantId from the user's claims.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal representing the authenticated user</param>
    /// <returns>The ApplicantId if found, otherwise null</returns>
    public static Guid? GetApplicantId(this ClaimsPrincipal? user)
    {
        if (user == null)
        {
            return null;
        }

        var claim = user.FindFirst(ApplicantIdClaimType);
        if (claim != null && Guid.TryParse(claim.Value, out var id))
        {
            return id;
        }

        return null;
    }

    /// <summary>
    /// Gets the user's email from claims.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal representing the authenticated user</param>
    /// <returns>The email if found, otherwise null</returns>
    public static string? GetEmail(this ClaimsPrincipal? user)
    {
        return user?.FindFirst(ClaimTypes.Email)?.Value
            ?? user?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Checks if the user is in the Admin role.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal representing the authenticated user</param>
    /// <returns>True if the user is an admin, otherwise false</returns>
    public static bool IsAdmin(this ClaimsPrincipal? user)
    {
        return user?.IsInRole("Admin") ?? false;
    }
}
