using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

public class KillerQuestionViewModel
{
    public Guid JobId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int QuestionIndex { get; set; }
    public int TotalQuestions { get; set; }

    [Display(Name = "Your answer")]
    [Required]
    public string Answer { get; set; } = string.Empty;

    [Display(Name = "I meet this requirement")]
    public bool MeetsRequirement { get; set; }
    public bool SaveAsDraft { get; set; }
}
