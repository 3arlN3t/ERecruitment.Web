using System.Globalization;
using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

/// <summary>
/// Helper responsible for projecting persistent application data into
/// the admin master list snapshot format.
/// </summary>
internal static class MasterListProjection
{
    public static AdminMasterListEntry Create(JobApplication application, Applicant applicant)
    {
        var profile = applicant.Profile ?? new ApplicantProfile();
        var equity = applicant.EquityDeclaration;

        var applicantName = BuildApplicantName(profile, applicant.Email);
        var age = CalculateAge(profile.DateOfBirth);
        var gender = string.IsNullOrWhiteSpace(equity?.Gender) ? "Not declared" : equity!.Gender!.Trim();
        var race = string.IsNullOrWhiteSpace(equity?.Ethnicity) ? null : equity!.Ethnicity!.Trim();

        var disabilityFlag = profile.HasDisability ? "Yes" : "No";
        var disabilityNarrative = BuildDisabilityNarrative(profile, equity);

        var qualificationSummary = BuildQualificationSummary(profile);
        var experienceSummary = BuildExperienceSummary(profile);
        var comments = BuildComments(application);

        return new AdminMasterListEntry(
            application.Id,
            applicant.Id,
            applicantName,
            applicant.Email,
            race,
            age,
            gender,
            profile.HasDisability,
            disabilityFlag,
            disabilityNarrative,
            qualificationSummary,
            experienceSummary,
            comments,
            application.Status,
            application.CreatedAtUtc,
            application.SubmittedAtUtc);
    }

    private static string BuildApplicantName(ApplicantProfile profile, string fallbackEmail)
    {
        if (!string.IsNullOrWhiteSpace(profile.FirstName) || !string.IsNullOrWhiteSpace(profile.LastName))
        {
            return $"{profile.FirstName} {profile.LastName}".Trim();
        }

        return fallbackEmail;
    }

    private static int? CalculateAge(DateTime? dob)
    {
        if (dob is null)
        {
            return null;
        }

        var today = DateTime.UtcNow.Date;
        var birthDate = dob.Value.Date;
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
        {
            age--;
        }
        return Math.Max(age, 0);
    }

    private static string? BuildDisabilityNarrative(ApplicantProfile profile, EquityDeclaration? equity)
    {
        if (profile.HasDisability)
        {
            if (!string.IsNullOrWhiteSpace(profile.DisabilityDetails))
            {
                return profile.DisabilityDetails.Trim();
            }
            if (!string.IsNullOrWhiteSpace(equity?.DisabilityStatus))
            {
                return equity!.DisabilityStatus!.Trim();
            }
            return "Disability declared";
        }

        if (!string.IsNullOrWhiteSpace(equity?.DisabilityStatus))
        {
            return equity!.DisabilityStatus!.Trim();
        }

        return null;
    }

    private static string BuildQualificationSummary(ApplicantProfile profile)
    {
        if (profile.Qualifications == null || profile.Qualifications.Count == 0)
        {
            return "No formal qualifications recorded";
        }

        static int ParseYear(string? year)
        {
            return int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : int.MinValue;
        }

        var ordered = profile.Qualifications
            .OrderByDescending(q => ParseYear(q.YearObtained))
            .ThenBy(q => q.QualificationName, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select(q =>
            {
                if (!string.IsNullOrWhiteSpace(q.YearObtained))
                {
                    return $"{q.QualificationName} ({q.YearObtained})";
                }
                return q.QualificationName;
            })
            .ToList();

        var summary = string.Join(" • ", ordered);
        if (profile.Qualifications.Count > 3)
        {
            summary += $" +{profile.Qualifications.Count - 3} more";
        }

        return summary;
    }

    private static string BuildExperienceSummary(ApplicantProfile profile)
    {
        var segments = new List<string>();
        var publicYears = profile.PublicSectorYears ?? 0;
        var privateYears = profile.PrivateSectorYears ?? 0;
        var declaredYears = publicYears + privateYears;

        if (declaredYears > 0)
        {
            segments.Add($"{declaredYears} yr{(declaredYears == 1 ? string.Empty : "s")} total");
        }

        if (publicYears > 0)
        {
            segments.Add($"{publicYears} yr{(publicYears == 1 ? string.Empty : "s")} public");
        }

        if (privateYears > 0)
        {
            segments.Add($"{privateYears} yr{(privateYears == 1 ? string.Empty : "s")} private");
        }

        if (segments.Count == 0)
        {
            var calculated = CalculateExperienceFromTimeline(profile);
            if (calculated > 0.4)
            {
                var rounded = Math.Round(calculated, 1, MidpointRounding.AwayFromZero);
                segments.Add($"{rounded:0.0} yrs timeline");
            }
            else
            {
                segments.Add("Under 1 yr recorded");
            }
        }

        return string.Join(" • ", segments);
    }

    private static double CalculateExperienceFromTimeline(ApplicantProfile profile)
    {
        if (profile.WorkExperience == null || profile.WorkExperience.Count == 0)
        {
            return 0;
        }

        double totalYears = 0;
        foreach (var record in profile.WorkExperience)
        {
            var from = record.FromDate ?? record.ToDate ?? DateTime.UtcNow;
            var to = record.ToDate ?? DateTime.UtcNow;
            if (to < from)
            {
                continue;
            }

            totalYears += (to - from).TotalDays / 365.25;
        }

        return totalYears;
    }

    private static string BuildComments(JobApplication application)
    {
        if (application.ScreeningAnswers == null || application.ScreeningAnswers.Count == 0)
        {
            return "Prepare panel questions";
        }

        var ordered = application.ScreeningAnswers
            .OrderBy(a => a.Order)
            .Select(a => BuildPrompt(a))
            .Where(prompt => !string.IsNullOrWhiteSpace(prompt))
            .Take(3)
            .ToList();

        if (ordered.Count == 0)
        {
            return "Review application dossier";
        }

        var comment = string.Join(" | ", ordered);
        if (application.ScreeningAnswers.Count > 3)
        {
            comment += " | +additional prompts";
        }

        return comment;
    }

    private static string? BuildPrompt(ScreeningAnswer answer)
    {
        if (string.IsNullOrWhiteSpace(answer.Question))
        {
            return null;
        }

        var prefix = answer.MeetsRequirement switch
        {
            true => "Confirm",
            false => "Probe",
            _ => "Ask"
        };

        var question = answer.Question.Trim();
        if (question.Length > 70)
        {
            question = question[..67] + "...";
        }

        return $"{prefix}: {question}";
    }
}
