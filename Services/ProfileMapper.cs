using System;
using System.Collections.Generic;
using System.Linq;
using ERecruitment.Web.Models;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERecruitment.Web.Services;

public class ProfileMapper : IProfileMapper
{
    private const string DefaultEmailBodyTemplate = "Dear {{applicantName}},\n\nWe would like to update you that your application for {0} is now '{{{{status}}}}'.\n\nKind regards,\nRecruitment Team";

    public AdminApplicationStatusUpdateViewModel CreateStatusUpdateViewModel(JobApplication application, Applicant applicant)
    {
        var viewModel = new AdminApplicationStatusUpdateViewModel
        {
            ApplicationId = application.Id,
            ApplicantEmail = applicant.Email,
            ApplicantName = applicant.Profile?.FirstName ?? applicant.Email,
            JobTitle = application.JobTitle,
            CurrentStatus = application.Status,
            NewStatus = application.Status,
            RejectionReason = application.RejectionReason,
            SubmittedAtUtc = application.SubmittedAtUtc,
            AuditTrail = application.AuditTrail
                .OrderByDescending(a => a.TimestampUtc)
                .ToList(),
            StatusOptions = BuildStatusOptions(application.Status),
            EmailSubject = $"Application update: {application.JobTitle}",
            EmailBody = string.Format(DefaultEmailBodyTemplate, application.JobTitle)
        };

        return viewModel;
    }

    public void UpdateStatusViewModel(AdminApplicationStatusUpdateViewModel model, JobApplication application, Applicant applicant)
    {
        model.ApplicantEmail = applicant.Email;
        model.ApplicantName = applicant.Profile?.FirstName ?? applicant.Email;
        model.JobTitle = application.JobTitle;
        model.CurrentStatus = application.Status;
        model.StatusOptions = BuildStatusOptions(model.NewStatus);
        model.SubmittedAtUtc = application.SubmittedAtUtc;
        model.AuditTrail = application.AuditTrail
            .OrderByDescending(a => a.TimestampUtc)
            .ToList();

        if (model.NewStatus == ApplicationStatus.Rejected && string.IsNullOrWhiteSpace(model.RejectionReason))
        {
            model.RejectionReason = application.RejectionReason;
        }

        if (string.IsNullOrWhiteSpace(model.EmailSubject))
        {
            model.EmailSubject = $"Application update: {application.JobTitle}";
        }

        if (string.IsNullOrWhiteSpace(model.EmailBody))
        {
            model.EmailBody = string.Format(DefaultEmailBodyTemplate, application.JobTitle);
        }
    }

    public AdminApplicantProfileViewModel MapApplicantProfile(Applicant applicant, JobApplication application)
    {
        var profile = applicant.Profile;
        var equity = applicant.EquityDeclaration;

        return new AdminApplicantProfileViewModel
        {
            ApplicationId = application.Id,
            JobTitle = application.JobTitle,
            ApplicationStatus = application.Status,
            SubmittedAt = application.SubmittedAtUtc,
            RejectionReason = application.RejectionReason,

            ApplicantId = applicant.Id,
            Email = applicant.Email,
            FirstName = profile.FirstName ?? string.Empty,
            LastName = profile.LastName ?? string.Empty,

            DateOfBirth = profile.DateOfBirth,
            PhoneNumber = profile.PhoneNumber,
            Location = profile.Location,
            SaIdNumber = profile.SaIdNumber,
            PassportNumber = profile.PassportNumber,
            IsSouthAfrican = profile.IsSouthAfrican,
            Nationality = profile.Nationality,
            PreferredLanguage = profile.PreferredLanguage,
            ContactEmail = profile.ContactEmail,

            HasWorkPermit = profile.HasWorkPermit,
            WorkPermitDetails = profile.WorkPermitDetails,
            HasDisability = profile.HasDisability,
            DisabilityDetails = profile.DisabilityDetails,

            ReferenceNumber = profile.ReferenceNumber,
            DepartmentName = profile.DepartmentName,
            PositionName = profile.PositionName,
            AvailabilityNotice = profile.AvailabilityNotice,
            AvailabilityDate = profile.AvailabilityDate,

            HasCriminalRecord = profile.HasCriminalRecord,
            CriminalRecordDetails = profile.CriminalRecordDetails,
            HasPendingCase = profile.HasPendingCase,
            PendingCaseDetails = profile.PendingCaseDetails,
            DismissedForMisconduct = profile.DismissedForMisconduct,
            DismissedDetails = profile.DismissedDetails,
            PendingDisciplinaryCase = profile.PendingDisciplinaryCase,
            PendingDisciplinaryDetails = profile.PendingDisciplinaryDetails,
            ResignedPendingDisciplinary = profile.ResignedPendingDisciplinary,
            ResignedPendingDisciplinaryDetails = profile.ResignedPendingDisciplinaryDetails,
            DischargedForIllHealth = profile.DischargedForIllHealth,
            DischargedDetails = profile.DischargedDetails,

            BusinessWithState = profile.BusinessWithState,
            BusinessDetails = profile.BusinessDetails,
            WillRelinquishBusiness = profile.WillRelinquishBusiness,
            WillRelinquishBusinessPlan = null,

            PublicSectorYears = profile.PublicSectorYears,
            PrivateSectorYears = profile.PrivateSectorYears,
            ReappointmentCondition = profile.ReappointmentCondition,
            ReappointmentDepartment = profile.ReappointmentDepartment,
            ReappointmentConditionDetails = profile.ReappointmentConditionDetails,
            ProfessionalRegistrationDate = profile.ProfessionalRegistrationDate,
            ProfessionalInstitution = profile.ProfessionalInstitution,
            ProfessionalRegistrationNumber = profile.ProfessionalRegistrationNumber,

            EquityConsentGiven = equity?.ConsentGiven ?? false,
            Ethnicity = equity?.Ethnicity,
            Gender = equity?.Gender,
            DisabilityStatus = equity?.DisabilityStatus,

            Languages = profile.Languages,
            Qualifications = profile.Qualifications,
            WorkExperience = profile.WorkExperience,
            References = profile.References,
            ScreeningAnswers = application.ScreeningAnswers,

            CvDocument = profile.Cv != null ? new DocumentInfo
            {
                FileName = profile.Cv.FileName ?? string.Empty,
                ContentType = profile.Cv.ContentType ?? string.Empty,
                StorageToken = profile.Cv.StorageToken ?? string.Empty,
                ParsedSummary = profile.Cv.ParsedSummary,
                DocumentType = "CV"
            } : null,

            IdDocument = profile.IdDocument != null ? new DocumentInfo
            {
                FileName = profile.IdDocument.FileName ?? string.Empty,
                ContentType = profile.IdDocument.ContentType ?? string.Empty,
                StorageToken = profile.IdDocument.StorageToken ?? string.Empty,
                UploadedAtUtc = profile.IdDocument.UploadedAtUtc,
                DocumentType = "ID Document"
            } : null,

            QualificationDocument = profile.QualificationDocument != null ? new DocumentInfo
            {
                FileName = profile.QualificationDocument.FileName ?? string.Empty,
                ContentType = profile.QualificationDocument.ContentType ?? string.Empty,
                StorageToken = profile.QualificationDocument.StorageToken ?? string.Empty,
                UploadedAtUtc = profile.QualificationDocument.UploadedAtUtc,
                DocumentType = "Qualification Document"
            } : null,

            DriversLicenseDocument = profile.DriversLicenseDocument != null ? new DocumentInfo
            {
                FileName = profile.DriversLicenseDocument.FileName ?? string.Empty,
                ContentType = profile.DriversLicenseDocument.ContentType ?? string.Empty,
                StorageToken = profile.DriversLicenseDocument.StorageToken ?? string.Empty,
                UploadedAtUtc = profile.DriversLicenseDocument.UploadedAtUtc,
                DocumentType = "Drivers License"
            } : null,

            AdditionalDocument = profile.AdditionalDocument != null ? new DocumentInfo
            {
                FileName = profile.AdditionalDocument.FileName ?? string.Empty,
                ContentType = profile.AdditionalDocument.ContentType ?? string.Empty,
                StorageToken = profile.AdditionalDocument.StorageToken ?? string.Empty,
                UploadedAtUtc = profile.AdditionalDocument.UploadedAtUtc,
                DocumentType = "Additional Document"
            } : null,

            DeclarationAccepted = profile.DeclarationAccepted,
            DeclarationDate = profile.DeclarationDate,
            SignatureData = profile.SignatureData,

            AuditTrail = application.AuditTrail.OrderByDescending(a => a.TimestampUtc).ToList()
        };
    }

    private static IEnumerable<SelectListItem> BuildStatusOptions(ApplicationStatus selected)
    {
        var options = new[]
        {
            ApplicationStatus.Submitted,
            ApplicationStatus.Interview,
            ApplicationStatus.Offer,
            ApplicationStatus.Rejected,
            ApplicationStatus.Withdrawn
        };

        return options.Select(status => new SelectListItem
        {
            Text = status.ToDisplayLabel(),
            Value = status.ToString(),
            Selected = status == selected
        }).ToList();
    }
}

