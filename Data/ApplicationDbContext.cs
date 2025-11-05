using ERecruitment.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERecruitment.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<EmailDelivery> EmailDeliveries => Set<EmailDelivery>();
    public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var stringListConverter = new ValueConverter<List<string>, string>(
            v => string.Join('\u001f', v ?? new List<string>()),
            v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split('\u001f', StringSplitOptions.RemoveEmptyEntries).ToList());
        
        var stringListComparer = new ValueComparer<List<string>>(
            (l1, l2) => (l1 ?? new()).SequenceEqual(l2 ?? new()),
            l => l.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            l => new List<string>(l ?? new()));

        modelBuilder.Entity<Applicant>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Email).IsRequired();
            entity.Property(a => a.PasswordHash).IsRequired();

            entity.OwnsOne(a => a.Profile, owned =>
            {
                owned.Property(p => p.ReferenceNumber);
                owned.Property(p => p.DepartmentName);
                owned.Property(p => p.PositionName);
                owned.Property(p => p.AvailabilityNotice);
                owned.Property(p => p.AvailabilityDate);
                owned.Property(p => p.DateOfBirth);
                owned.Property(p => p.PassportNumber);
                owned.Property(p => p.HasDisability);
                owned.Property(p => p.IsSouthAfrican);
                owned.Property(p => p.Nationality);
                owned.Property(p => p.HasWorkPermit);
                owned.Property(p => p.WorkPermitDetails);
                owned.Property(p => p.HasCriminalRecord);
                owned.Property(p => p.CriminalRecordDetails);
                owned.Property(p => p.HasPendingCase);
                owned.Property(p => p.PendingCaseDetails);
                owned.Property(p => p.DismissedForMisconduct);
                owned.Property(p => p.DismissedDetails);
                owned.Property(p => p.PendingDisciplinaryCase);
                owned.Property(p => p.PendingDisciplinaryDetails);
                owned.Property(p => p.ResignedPendingDisciplinary);
                owned.Property(p => p.ResignedPendingDisciplinaryDetails);
                owned.Property(p => p.DischargedForIllHealth);
                owned.Property(p => p.DischargedDetails);
                owned.Property(p => p.BusinessWithState);
                owned.Property(p => p.BusinessDetails);
                owned.Property(p => p.WillRelinquishBusiness);
                owned.Property(p => p.PublicSectorYears);
                owned.Property(p => p.PrivateSectorYears);
                owned.Property(p => p.ReappointmentCondition);
                owned.Property(p => p.ReappointmentDepartment);
                owned.Property(p => p.ReappointmentConditionDetails);
                owned.Property(p => p.ProfessionalRegistrationDate);
                owned.Property(p => p.ProfessionalInstitution);
                owned.Property(p => p.ProfessionalRegistrationNumber);
                owned.Property(p => p.PreferredLanguage);
                owned.Property(p => p.ContactEmail);
                owned.Property(p => p.DeclarationAccepted);
                owned.Property(p => p.DeclarationDate);
                owned.Property(p => p.SignatureData);

                owned.Property(p => p.FirstName);
                owned.Property(p => p.LastName);
                owned.Property(p => p.PhoneNumber);
                owned.Property(p => p.Location);
                owned.Property(p => p.SaIdNumber);

                owned.OwnsOne(p => p.Cv, cv =>
                {
                    cv.Property(c => c.FileName);
                    cv.Property(c => c.ContentType);
                    cv.Property(c => c.StorageToken);
                    cv.Property(c => c.ParsedSummary);
                });

                owned.OwnsOne(p => p.IdDocument, doc =>
                {
                    doc.Property(d => d.FileName);
                    doc.Property(d => d.ContentType);
                    doc.Property(d => d.StorageToken);
                    doc.Property(d => d.UploadedAtUtc);
                    doc.Property(d => d.DocumentType);
                });

                owned.OwnsOne(p => p.QualificationDocument, doc =>
                {
                    doc.Property(d => d.FileName);
                    doc.Property(d => d.ContentType);
                    doc.Property(d => d.StorageToken);
                    doc.Property(d => d.UploadedAtUtc);
                    doc.Property(d => d.DocumentType);
                });

                owned.OwnsOne(p => p.DriversLicenseDocument, doc =>
                {
                    doc.Property(d => d.FileName);
                    doc.Property(d => d.ContentType);
                    doc.Property(d => d.StorageToken);
                    doc.Property(d => d.UploadedAtUtc);
                    doc.Property(d => d.DocumentType);
                });

                owned.OwnsOne(p => p.AdditionalDocument, doc =>
                {
                    doc.Property(d => d.FileName);
                    doc.Property(d => d.ContentType);
                    doc.Property(d => d.StorageToken);
                    doc.Property(d => d.UploadedAtUtc);
                    doc.Property(d => d.DocumentType);
                });

                owned.OwnsMany(p => p.Languages, nav =>
                {
                    nav.WithOwner().HasForeignKey("ApplicantProfileId");
                    nav.Property(l => l.LanguageName);
                    nav.Property(l => l.SpeakProficiency);
                    nav.Property(l => l.ReadWriteProficiency);
                });

                owned.OwnsMany(p => p.Qualifications, nav =>
                {
                    nav.WithOwner().HasForeignKey("ApplicantProfileId");
                    nav.Property(q => q.InstitutionName);
                    nav.Property(q => q.QualificationName);
                    nav.Property(q => q.StudentNumber);
                    nav.Property(q => q.YearObtained);
                    nav.Property(q => q.Status);
                });

                owned.OwnsMany(p => p.WorkExperience, nav =>
                {
                    nav.WithOwner().HasForeignKey("ApplicantProfileId");
                    nav.Property(w => w.EmployerName);
                    nav.Property(w => w.PositionHeld);
                    nav.Property(w => w.FromDate);
                    nav.Property(w => w.ToDate);
                    nav.Property(w => w.Status);
                    nav.Property(w => w.ReasonForLeaving);
                });

                owned.OwnsMany(p => p.References, nav =>
                {
                    nav.WithOwner().HasForeignKey("ApplicantProfileId");
                    nav.Property(r => r.Name);
                    nav.Property(r => r.Relationship);
                    nav.Property(r => r.ContactNumber);
                });
            });

            entity.OwnsOne(a => a.EquityDeclaration, owned =>
            {
                owned.Property(e => e.ConsentGiven);
                owned.Property(e => e.Ethnicity);
                owned.Property(e => e.Gender);
                owned.Property(e => e.DisabilityStatus);
            });

            entity.HasMany(a => a.Applications)
                .WithOne()
                .HasForeignKey(app => app.ApplicantId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<JobPosting>(entity =>
        {
            entity.HasKey(j => j.Id);
            
            // Primary identification - required fields
            entity.Property(j => j.PostNumber).IsRequired().HasMaxLength(50);
            entity.Property(j => j.ReferenceNumber).IsRequired().HasMaxLength(50);
            entity.Property(j => j.Title).IsRequired().HasMaxLength(300);
            
            // Salary section
            entity.Property(j => j.SalaryRange).IsRequired().HasMaxLength(2000);
            
            // Centre section
            entity.Property(j => j.Centre).IsRequired().HasMaxLength(200);
            entity.Property(j => j.Province).HasMaxLength(50);
            
            // Requirements section
            entity.Property(j => j.Requirements).IsRequired().HasMaxLength(10000);
            
            // Duties section
            entity.Property(j => j.DutiesDescription).IsRequired().HasMaxLength(10000);
            
            // Enquiries section
            entity.Property(j => j.EnquiriesContactPerson).IsRequired().HasMaxLength(200);
            entity.Property(j => j.EnquiriesPhone).IsRequired().HasMaxLength(50);
            
            // Applications section
            entity.Property(j => j.ApplicationEmail).IsRequired().HasMaxLength(200);
            
            // Notes section
            entity.Property(j => j.AdditionalNotes).HasMaxLength(2000);
            
            // Legacy fields (for backward compatibility)
            entity.Property(j => j.Department).IsRequired().HasMaxLength(200);
            entity.Property(j => j.Location).IsRequired().HasMaxLength(200);
            entity.Property(j => j.Description).IsRequired().HasMaxLength(5000);
            
            // Metadata
            entity.Property(j => j.PostedByUserId).HasMaxLength(450);
            
            // List converter for KillerQuestions
            entity.Property(j => j.KillerQuestions)
                .HasConversion(stringListConverter)
                .Metadata.SetValueComparer(stringListComparer);
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.JobTitle).IsRequired();
            entity.Property(a => a.Status).HasConversion<int>();

            // Unique constraint: One active application per job per applicant
            // Allows reapplication after withdrawal (Status != 5 where 5 = Withdrawn)
            entity.HasIndex(a => new { a.ApplicantId, a.JobPostingId })
                .IsUnique()
                .HasDatabaseName("IX_JobApplications_ApplicantId_JobPostingId_Unique")
                .HasFilter("[Status] != 5");

            entity.OwnsMany(a => a.ScreeningAnswers, owned =>
            {
                owned.WithOwner();
                owned.Property(sa => sa.Order);
                owned.Property(sa => sa.Question);
                owned.Property(sa => sa.Answer);
                owned.Property(sa => sa.MeetsRequirement);
            });

            entity.HasMany(a => a.AuditTrail)
                .WithOne()
                .HasForeignKey(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedNever();
            entity.Property(a => a.Actor).IsRequired();
            entity.Property(a => a.Action).IsRequired();
            entity.HasIndex(a => new { a.JobApplicationId, a.TimestampUtc });
        });

        modelBuilder.Entity<EmailDelivery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ToEmail).IsRequired();
            entity.Property(e => e.Subject).IsRequired();
        });

        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Email).IsRequired();
            entity.Property(p => p.Token).IsRequired();
            entity.HasIndex(p => p.Token);
        });
    }
}


