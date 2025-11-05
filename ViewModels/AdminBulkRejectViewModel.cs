using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

public class AdminBulkRejectViewModel
{
    [Required]
    [MinLength(1, ErrorMessage = "Select at least one application.")]
    public List<Guid> SelectedApplicationIds { get; set; } = new();

    [Required]
    [Display(Name = "Email template")]
    [MinLength(10, ErrorMessage = "Template should be at least 10 characters")]
    public string TemplateBody { get; set; } = string.Empty;
}
