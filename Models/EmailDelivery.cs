namespace ERecruitment.Web.Models;

public enum EmailDeliveryStatus
{
    Sent,
    Failed
}

public class EmailDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public required string ToEmail { get; set; }
    public required string Subject { get; set; }
    public string BodyPreview { get; set; } = string.Empty;
    public string FullBody { get; set; } = string.Empty;
    public EmailDeliveryStatus Status { get; set; } = EmailDeliveryStatus.Sent;
    public string? Error { get; set; }
}


