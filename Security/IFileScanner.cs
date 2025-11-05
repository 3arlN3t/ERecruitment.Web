using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Security;

public interface IFileScanner
{
    Task<bool> IsSafeAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<bool> IsSafeAsync(Stream stream, CancellationToken cancellationToken = default);
}


