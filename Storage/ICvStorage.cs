using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Storage;

public interface ICvStorage
{
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageToken, CancellationToken cancellationToken = default);
    bool Exists(string storageToken);
}


