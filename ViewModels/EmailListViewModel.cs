using ERecruitment.Web.Models;

namespace ERecruitment.Web.ViewModels;

public class EmailListViewModel
{
    public required IReadOnlyCollection<EmailDelivery> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? ToFilter { get; set; }
    public string? SubjectFilter { get; set; }
    public EmailDeliveryStatus? StatusFilter { get; set; }
}


