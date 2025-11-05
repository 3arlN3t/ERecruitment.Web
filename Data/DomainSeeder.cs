using ERecruitment.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ERecruitment.Web.Data;

public static class DomainSeeder
{
    public static void EnsureSeeded(ApplicationDbContext db)
    {
        // Ensure database and schema are created using migrations
        db.Database.Migrate();

        SeedJobPostings(db);
        SeedApplicants(db);
        SeedJobApplications(db);
    }

    private static void SeedJobPostings(ApplicationDbContext db)
    {
        var seedJobPostings = new[]
        {
            new JobPosting
            {
                PostNumber = "14/001",
                ReferenceNumber = "DT 01/2025",
                Title = "Digital Transformation Lead",
                Department = "Strategy & Innovation",
                Location = "Pretoria",
                Centre = "National Office",
                Province = "Gauteng",
                SalaryRange = "R1 022 226 – R1 205 233 per annum (Level 13)",
                Requirements = "Bachelor's degree in Information Systems or related field. 7 years’ experience leading digital programmes.",
                DutiesDescription = "Lead the department’s digital modernisation roadmap, oversee agile delivery teams, ensure compliance.",
                ApplicationEmail = "recruitment@tourism.gov.za",
                Description = "Drive digital transformation initiatives across the department.",
                KillerQuestions = { "Provide an example of a transformation programme you led end-to-end." }
            },
            new JobPosting
            {
                PostNumber = "14/002",
                ReferenceNumber = "DT 02/2025",
                Title = "Senior Tourism Economist",
                Department = "Policy & Research",
                Location = "Cape Town",
                Centre = "Western Cape Office",
                Province = "Western Cape",
                SalaryRange = "R908 502 per annum (Level 12)",
                Requirements = "Postgraduate qualification in Economics. 5 years’ experience in economic modelling.",
                DutiesDescription = "Develop tourism economic models, advise on policy, publish quarterly insights.",
                ApplicationEmail = "economics@tourism.gov.za",
                Description = "Provide strategic economic insights for the tourism sector.",
                KillerQuestions = { "Describe a tourism impact model you have built or contributed to." }
            },
            new JobPosting
            {
                PostNumber = "14/003",
                ReferenceNumber = "DT 03/2025",
                Title = "HR Business Partner",
                Department = "Corporate Services",
                Location = "Durban",
                Centre = "KwaZulu-Natal Regional Office",
                Province = "KwaZulu-Natal",
                SalaryRange = "R811 560 per annum (Level 11)",
                Requirements = "BCom HR or equivalent. 6 years’ HR experience with 3 years partnering senior leadership.",
                DutiesDescription = "Partner with line managers, drive workforce planning, champion change initiatives.",
                ApplicationEmail = "hr@tourism.gov.za",
                Description = "Align HR strategy with business objectives across multiple branches.",
                KillerQuestions = { "How have you used analytics to drive HR decisions?" }
            },
            new JobPosting
            {
                PostNumber = "14/004",
                ReferenceNumber = "DT 04/2025",
                Title = "Infrastructure Project Manager",
                Department = "Tourism Infrastructure",
                Location = "Polokwane",
                Centre = "Limpopo Cluster",
                Province = "Limpopo",
                SalaryRange = "R744 255 per annum (Level 10)",
                Requirements = "Engineering or Construction Management degree. PMP certification advantageous.",
                DutiesDescription = "Deliver tourism infrastructure upgrades, manage vendors, track milestones.",
                ApplicationEmail = "projects@tourism.gov.za",
                Description = "Oversee provincial tourism infrastructure initiatives.",
                KillerQuestions = { "Share an example of delivering a complex capital project on time." }
            },
            new JobPosting
            {
                PostNumber = "14/005",
                ReferenceNumber = "DT 05/2025",
                Title = "Grants Compliance Officer",
                Department = "Finance & Governance",
                Location = "Kimberley",
                Centre = "Northern Cape Office",
                Province = "Northern Cape",
                SalaryRange = "R557 478 per annum (Level 8)",
                Requirements = "BCom Accounting or equivalent. Knowledge of PFMA and grant frameworks.",
                DutiesDescription = "Monitor grant utilisation, prepare compliance reports, support audits.",
                ApplicationEmail = "finance@tourism.gov.za",
                Description = "Ensure governance of tourism grant programmes.",
                KillerQuestions = { "Describe a time you improved compliance controls in a grants environment." }
            },
            new JobPosting
            {
                PostNumber = "14/006",
                ReferenceNumber = "DT 06/2025",
                Title = "ICT Service Desk Manager",
                Department = "Information Technology",
                Location = "Bloemfontein",
                Centre = "Free State Shared Services",
                Province = "Free State",
                SalaryRange = "R473 112 per annum (Level 9)",
                Requirements = "Diploma in IT with 5 years’ service desk leadership.",
                DutiesDescription = "Lead service desk team, implement ITIL processes, manage vendor SLAs.",
                ApplicationEmail = "it-support@tourism.gov.za",
                Description = "Ensure reliable ICT support for field offices.",
                KillerQuestions = { "How have you improved first-contact resolution in your team?" }
            }
        };

        foreach (var job in seedJobPostings)
        {
            if (!db.JobPostings.Any(j => j.ReferenceNumber == job.ReferenceNumber))
            {
                db.JobPostings.Add(job);
            }
        }

        db.SaveChanges();
    }

    private static void SeedApplicants(ApplicationDbContext db)
    {
        if (db.Applicants.Count() >= 12)
        {
            return;
        }

        var seedApplicants = new[]
        {
            new Applicant
            {
                Email = "lerato.khumalo@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Lerato", "Khumalo", "1986-04-12", "Johannesburg", "8204120456088", "Project Management Specialist", true),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "African", Gender = "Female", DisabilityStatus = "None declared" }
            },
            new Applicant
            {
                Email = "michael.naidoo@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Michael", "Naidoo", "1983-11-02", "Durban", "8311025055089", "Infrastructure Project Lead", false),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "Indian", Gender = "Male", DisabilityStatus = "None declared" }
            },
            new Applicant
            {
                Email = "amanda.Visser@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Amanda", "Visser", "1990-02-24", "Cape Town", "9002240086082", "Tourism Economist", false),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "White", Gender = "Female", DisabilityStatus = "None declared" }
            },
            new Applicant
            {
                Email = "tshepo.morake@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Tshepo", "Morake", "1988-07-18", "Polokwane", "8807185154081", "Civil Engineer", false),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "African", Gender = "Male", DisabilityStatus = "Mobility impairment" }
            },
            new Applicant
            {
                Email = "chantelle.geldenhuys@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Chantelle", "Geldenhuys", "1992-09-09", "Bloemfontein", "9209090184080", "ICT Service Delivery Manager", false),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "White", Gender = "Female", DisabilityStatus = "None declared" }
            },
            new Applicant
            {
                Email = "peter.molefe@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Peter", "Molefe", "1979-05-30", "Kimberley", "7905305144089", "Senior Accountant", false),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "African", Gender = "Male", DisabilityStatus = "None declared" }
            },
            new Applicant
            {
                Email = "zanele.dlamini@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Zanele", "Dlamini", "1993-03-14", "Pretoria", "9303145084082", "Digital Strategist", false),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "African", Gender = "Female", DisabilityStatus = "None declared" }
            },
            new Applicant
            {
                Email = "siyabonga.khoza@test.com",
                PasswordHash = "seed",
                Profile = BuildProfile("Siyabonga", "Khoza", "1985-01-21", "East London", "8501215804085", "Grants Compliance Analyst", false),
                EquityDeclaration = new EquityDeclaration { ConsentGiven = true, Ethnicity = "African", Gender = "Male", DisabilityStatus = "None declared" }
            },
        };

        foreach (var applicant in seedApplicants)
        {
            if (!db.Applicants.Any(a => a.Email == applicant.Email))
            {
                db.Applicants.Add(applicant);
            }
        }

        db.SaveChanges();
    }

    private static ApplicantProfile BuildProfile(
        string firstName,
        string lastName,
        string dobIso,
        string location,
        string idNumber,
        string headline,
        bool hasDisability)
    {
        return new ApplicantProfile
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = DateTime.Parse(dobIso),
            SaIdNumber = idNumber,
            Location = location,
            PhoneNumber = "+27 82 000 0000",
            ContactEmail = $"{firstName.ToLower()}.{lastName.ToLower()}@test.com",
            AvailabilityNotice = "30 days",
            HasDisability = hasDisability,
            DisabilityDetails = hasDisability ? "Mobility impairment – requires accessible facilities" : null,
            Qualifications = new List<QualificationRecord>
            {
                new QualificationRecord
                {
                    InstitutionName = "University of Pretoria",
                    QualificationName = $"{headline} (Honours)",
                    YearObtained = "2014",
                    Status = "Completed"
                }
            },
            WorkExperience = new List<WorkExperienceRecord>
            {
                new WorkExperienceRecord
                {
                    EmployerName = "Tourism South Africa",
                    PositionHeld = headline,
                    FromDate = DateTime.UtcNow.AddYears(-5),
                    ToDate = null,
                    Status = "Current"
                }
            },
            DeclarationAccepted = true,
            DeclarationDate = DateTime.UtcNow.AddYears(-1)
        };
    }

    private static void SeedJobApplications(ApplicationDbContext db)
    {
        if (db.JobApplications.Count() >= 20)
        {
            return;
        }

        var jobs = db.JobPostings.ToList();
        var applicants = db.Applicants.ToList();
        if (jobs.Count == 0 || applicants.Count == 0)
        {
            return;
        }

        var statuses = new[]
        {
            ApplicationStatus.Submitted,
            ApplicationStatus.Interview,
            ApplicationStatus.Offer,
            ApplicationStatus.Rejected,
            ApplicationStatus.Draft,
            ApplicationStatus.Submitted,
            ApplicationStatus.Interview,
            ApplicationStatus.Submitted
        };

        var applicationsToAdd = new List<JobApplication>();
        var pairs = jobs.SelectMany(j => applicants, (job, applicant) => new { job, applicant })
                        .Take(32)
                        .ToList();

        for (var index = 0; index < pairs.Count; index++)
        {
            var pair = pairs[index];
            if (db.JobApplications.Any(a => a.JobPostingId == pair.job.Id && a.ApplicantId == pair.applicant.Id))
            {
                continue;
            }

            var createdAt = DateTime.UtcNow.AddDays(-10 + index);
            var status = statuses[index % statuses.Length];
            var submittedAt = status == ApplicationStatus.Draft ? (DateTime?)null : createdAt.AddHours(4);

            var application = new JobApplication
            {
                ApplicantId = pair.applicant.Id,
                JobPostingId = pair.job.Id,
                JobTitle = pair.job.Title,
                Status = status,
                CreatedAtUtc = createdAt,
                SubmittedAtUtc = submittedAt,
                ScreeningAnswers = new List<ScreeningAnswer>
                {
                    new ScreeningAnswer
                    {
                        Order = 1,
                        Question = "Explain why you are suitable for this role.",
                        Answer = "I have led similar programmes with measurable outcomes.",
                        MeetsRequirement = true
                    },
                    new ScreeningAnswer
                    {
                        Order = 2,
                        Question = "What is your earliest availability?",
                        Answer = "30 days notice.",
                        MeetsRequirement = true
                    }
                }
            };

            applicationsToAdd.Add(application);

            if (applicationsToAdd.Count >= 24)
            {
                break;
            }
        }

        if (applicationsToAdd.Count > 0)
        {
            db.JobApplications.AddRange(applicationsToAdd);
            db.SaveChanges();
        }
    }
}


