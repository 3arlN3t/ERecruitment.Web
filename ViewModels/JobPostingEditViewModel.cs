using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

public class JobPostingEditViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(200)]
    public string? Title { get; set; }

    [Required]
    [StringLength(200)]
    public string? Department { get; set; }

    [Required]
    [StringLength(200)]
    public string? Location { get; set; }

    [Required]
    [StringLength(4000)]
    public string? Description { get; set; }

    [Display(Name = "Killer questions (one per line)")]
    public string? KillerQuestionsText { get; set; }
}


