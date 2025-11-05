namespace ERecruitment.Web.Models;

public class CvDocument
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required string StorageToken { get; set; }
    public string? ParsedSummary { get; set; }
    public bool ParsedSuccessfully => ParsedSummary is not null;
}
