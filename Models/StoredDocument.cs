namespace ERecruitment.Web.Models;

public class StoredDocument
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StorageToken { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public string DocumentType { get; set; } = string.Empty;
}
