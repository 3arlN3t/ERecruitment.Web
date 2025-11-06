using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ERecruitment.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERecruitment.Web.ViewModels;

public class AdminApplicationStatusUpdateViewModel
{
    public Guid ApplicationId { get; set; }

    [Display(Name = "Applicant email")]
    public string ApplicantEmail { get; set; } = string.Empty;

    [Display(Name = "Applicant name")]
    public string? ApplicantName { get; set; }

    [Display(Name = "Position")]
    public string JobTitle { get; set; } = string.Empty;

    public ApplicationStatus CurrentStatus { get; set; }

    [Required]
    [Display(Name = "New status")]
    public ApplicationStatus NewStatus { get; set; }

    [Display(Name = "Internal note")]
    [DataType(DataType.MultilineText)]
    public string? Note { get; set; }

    [Display(Name = "Rejection reason")]
    [DataType(DataType.MultilineText)]
    public string? RejectionReason { get; set; }

    [Display(Name = "Send email notification to applicant")]
    public bool SendEmail { get; set; }

    [Display(Name = "Email subject")]
    public string? EmailSubject { get; set; }

    [Display(Name = "Email body")]
    [DataType(DataType.MultilineText)]
    public string? EmailBody { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public IReadOnlyCollection<AuditEntry> AuditTrail { get; set; } = Array.Empty<AuditEntry>();

    public IEnumerable<SelectListItem> StatusOptions { get; set; } = Array.Empty<SelectListItem>();

    public string? ReturnUrl { get; set; }
}
