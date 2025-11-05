namespace ERecruitment.Web.Models;

public class ScreeningAnswer
{
    public int Order { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public bool? MeetsRequirement { get; set; }
}


