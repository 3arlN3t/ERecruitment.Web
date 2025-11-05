using Microsoft.AspNetCore.Http;
using ERecruitment.Web.Security;

namespace ERecruitment.Web.Storage;

public class LocalDiskCvStorage : ICvStorage
{
    private readonly string _root;
    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx"
    };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IFileScanner _scanner;

    public LocalDiskCvStorage(IWebHostEnvironment env, IFileScanner scanner)
    {
        _root = Path.Combine(env.ContentRootPath, "App_Data", "cvs");
        Directory.CreateDirectory(_root);
        _scanner = scanner;
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0 || file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("Invalid file size.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (!_allowedExtensions.Contains(ext))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        // basic content scanning
        if (!await _scanner.IsSafeAsync(file, cancellationToken))
        {
            throw new InvalidOperationException("File failed safety scan.");
        }

        var token = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
        var path = Path.Combine(_root, token);
        await using var stream = File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return token;
    }

    public Task<Stream> OpenReadAsync(string storageToken, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_root, storageToken);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException();
        }
        Stream s = File.OpenRead(path);
        return Task.FromResult(s);
    }

    public bool Exists(string storageToken)
    {
        var path = Path.Combine(_root, storageToken);
        return File.Exists(path);
    }
}


