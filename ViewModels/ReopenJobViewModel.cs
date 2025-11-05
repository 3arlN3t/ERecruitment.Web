namespace ERecruitment.Web.ViewModels;

public class ReopenJobViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? CurrentClosingDate { get; set; }
    public DateTime NewClosingDate { get; set; }
}
