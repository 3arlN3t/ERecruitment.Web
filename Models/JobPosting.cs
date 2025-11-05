namespace ERecruitment.Web.Models;

/// <summary>
/// Government-compliant job posting model following official recruitment document structure.
/// Matches the format used in government job advertisements.
/// </summary>
public class JobPosting
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // PRIMARY IDENTIFICATION
    public string PostNumber { get; set; } = string.Empty;        // e.g., "36/111"
    public string ReferenceNumber { get; set; } = string.Empty;   // e.g., "DT 14/2025"
    public string Title { get; set; } = string.Empty;            // e.g., "DEPUTY DIRECTOR: INFORMATION TECHNOLOGY AUDIT"

    // SECTION 1: SALARY
    public string SalaryRange { get; set; } = string.Empty;       // Full salary information: "R896 436 per annum, (all-inclusive remuneration package consisting of a basic salary, the State's contribution to the Government Employees Pension Fund and a flexible portion that may be structured according to the MMS dispensation)"

    // SECTION 2: CENTRE
    public string Centre { get; set; } = string.Empty;            // e.g., "Pretoria"
    public string? Province { get; set; }                         // Optional: "Gauteng"

    // SECTION 3: REQUIREMENTS
    public string Requirements { get; set; } = string.Empty;  // Complete requirements: Qualifications, Experience, Certifications, Knowledge, Skills, and Other requirements

    // SECTION 4: DUTIES
    public string DutiesDescription { get; set; } = string.Empty;  // Full responsibilities (rich text)

    // SECTION 5: ENQUIRIES
    public string EnquiriesContactPerson { get; set; } = string.Empty;  // e.g., "Ms T Sibiya"
    public string EnquiriesPhone { get; set; } = string.Empty;          // e.g., "(012) 444 6291"

    // SECTION 6: APPLICATIONS
    public string ApplicationEmail { get; set; } = string.Empty;     // e.g., "Recruitment14@tourism.gov.za"
    public DateTime? ClosingDate { get; set; }

    // SECTION 7: NOTE
    public string? AdditionalNotes { get; set; }        // Other compliance info

    // METADATA & LEGACY FIELDS (for backward compatibility)
    public string Department { get; set; } = string.Empty;         // Legacy field, maps to department/ministry
    public string Location { get; set; } = string.Empty;           // Legacy field, maps to Centre
    public string Description { get; set; } = string.Empty;        // Legacy field, can be generated from Requirements+Duties
    public List<string> KillerQuestions { get; set; } = new();    // Screening questions

    // METADATA
    public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    public DateTime DateLastModified { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? PostedByUserId { get; set; }

    /// <summary>
    /// Determines if the job posting is expired based on the closing date.
    /// </summary>
    /// <returns>True if the closing date has passed, false otherwise.</returns>
    public bool IsExpired => ClosingDate.HasValue && ClosingDate.Value < DateTime.UtcNow;

    /// <summary>
    /// Determines if the job is accepting applications.
    /// A job accepts applications if it's active and not expired.
    /// </summary>
    public bool IsAcceptingApplications => IsActive && !IsExpired;
}
