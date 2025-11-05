using System.ComponentModel.DataAnnotations;

namespace ERecruitment.Web.ViewModels;

public class AdminLoginViewModel
{
    [Required]
    [Display(Name = "Access code")]
    public string AccessCode { get; set; } = string.Empty;
}
