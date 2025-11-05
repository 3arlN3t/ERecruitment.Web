using ERecruitment.Web.Models;

namespace ERecruitment.Web.Utilities;

/// <summary>
/// Extension methods for ApplicantProfile to calculate completion metrics
/// </summary>
public static class ProfileExtensions
{
    /// <summary>
    /// Calculates the percentage of profile completion based on key fields.
    /// Returns a value between 0 and 100.
    /// </summary>
    public static int CalculateCompletionPercentage(this ApplicantProfile profile)
    {
        var totalFields = 0;
        var completedFields = 0;

        // Core Personal Information (Weight: 30%)
        totalFields += 6;
        if (!string.IsNullOrWhiteSpace(profile.FirstName)) completedFields++;
        if (!string.IsNullOrWhiteSpace(profile.LastName)) completedFields++;
        if (profile.DateOfBirth.HasValue) completedFields++;
        if (!string.IsNullOrWhiteSpace(profile.PhoneNumber)) completedFields++;
        if (!string.IsNullOrWhiteSpace(profile.Location)) completedFields++;
        if (!string.IsNullOrWhiteSpace(profile.SaIdNumber) || !string.IsNullOrWhiteSpace(profile.PassportNumber)) completedFields++;

        // Contact & Availability (Weight: 10%)
        totalFields += 2;
        if (!string.IsNullOrWhiteSpace(profile.ContactEmail)) completedFields++;
        if (profile.AvailabilityDate.HasValue || !string.IsNullOrWhiteSpace(profile.AvailabilityNotice)) completedFields++;

        // Employment History (Weight: 15%)
        totalFields += 3;
        if (profile.PublicSectorYears.HasValue || profile.PrivateSectorYears.HasValue) completedFields++;
        if (profile.WorkExperience.Any()) completedFields++;
        if (profile.WorkExperience.Count >= 2) completedFields++; // Bonus for multiple entries

        // Qualifications (Weight: 15%)
        totalFields += 3;
        if (profile.Qualifications.Any()) completedFields++;
        if (profile.Qualifications.Count >= 2) completedFields++; // Bonus for multiple entries
        if (profile.QualificationDocument != null) completedFields++;

        // Documents (Weight: 20%)
        totalFields += 4;
        if (profile.Cv != null) completedFields++;
        if (profile.IdDocument != null) completedFields++;
        if (profile.QualificationDocument != null) completedFields++; // Already counted but important
        if (profile.DriversLicenseDocument != null || profile.AdditionalDocument != null) completedFields++;

        // References (Weight: 10%)
        totalFields += 2;
        if (profile.References.Any()) completedFields++;
        if (profile.References.Count >= 2) completedFields++; // Minimum 2 references recommended

        // Declaration (Weight: Must have)
        totalFields += 1;
        if (profile.DeclarationAccepted) completedFields++;

        // Languages (Bonus)
        totalFields += 1;
        if (profile.Languages.Any()) completedFields++;

        var percentage = (int)Math.Round((double)completedFields / totalFields * 100);
        return Math.Clamp(percentage, 0, 100);
    }

    /// <summary>
    /// Returns a user-friendly completion status message
    /// </summary>
    public static string GetCompletionStatus(this ApplicantProfile profile)
    {
        var percentage = profile.CalculateCompletionPercentage();

        return percentage switch
        {
            100 => "Complete",
            >= 80 => "Almost Complete",
            >= 60 => "In Progress",
            >= 40 => "Partially Complete",
            >= 20 => "Getting Started",
            _ => "Just Started"
        };
    }

    /// <summary>
    /// Returns the CSS class for the completion badge
    /// </summary>
    public static string GetCompletionBadgeClass(this ApplicantProfile profile)
    {
        var percentage = profile.CalculateCompletionPercentage();

        return percentage switch
        {
            100 => "bg-success",
            >= 80 => "bg-info",
            >= 60 => "bg-primary",
            >= 40 => "bg-warning",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Gets missing critical fields that should be completed
    /// </summary>
    public static List<string> GetMissingCriticalFields(this ApplicantProfile profile)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.FirstName)) missing.Add("First Name");
        if (string.IsNullOrWhiteSpace(profile.LastName)) missing.Add("Last Name");
        if (!profile.DateOfBirth.HasValue) missing.Add("Date of Birth");
        if (string.IsNullOrWhiteSpace(profile.PhoneNumber)) missing.Add("Phone Number");
        if (string.IsNullOrWhiteSpace(profile.SaIdNumber) && string.IsNullOrWhiteSpace(profile.PassportNumber))
            missing.Add("ID Number or Passport");
        if (profile.Cv == null) missing.Add("CV/Resume");
        if (!profile.Qualifications.Any()) missing.Add("Qualifications");
        if (!profile.WorkExperience.Any()) missing.Add("Work Experience");
        if (!profile.DeclarationAccepted) missing.Add("Declaration Acceptance");

        return missing;
    }

    /// <summary>
    /// Checks if profile meets minimum requirements for job applications
    /// </summary>
    public static bool MeetsMinimumRequirements(this ApplicantProfile profile)
    {
        return profile.CalculateCompletionPercentage() >= 60 &&
               !string.IsNullOrWhiteSpace(profile.FirstName) &&
               !string.IsNullOrWhiteSpace(profile.LastName) &&
               profile.DeclarationAccepted;
    }
}
