namespace ERecruitment.Web.ViewModels;

/// <summary>
/// View model for error display pages.
/// </summary>
public class ErrorViewModel
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public string? RequestPath { get; set; }
    public DateTime ErrorTime { get; set; } = DateTime.UtcNow;

    public bool ShowTraceId => !string.IsNullOrEmpty(TraceId);
    public bool ShowRequestPath => !string.IsNullOrEmpty(RequestPath);
}

