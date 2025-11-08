namespace ERecruitment.Web.Services;

public record FileDownloadResult(byte[] Content, string ContentType, string FileName);

