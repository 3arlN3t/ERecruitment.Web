using System;
using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ERecruitment.Web.Security;

public static class SecurityDiagnostics
{
    private const int ZipBombPayloadSize = 60 * 1024 * 1024; // 60 MB uncompressed
    private const string ZipBombEntryName = "word/document.xml";

    public static async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment())
        {
            return;
        }

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SecurityDiagnostics");
        var scanner = scope.ServiceProvider.GetRequiredService<IFileScanner>();

        if (!await EnsureZipBombDetectionAsync(scanner, cancellationToken))
        {
            logger.LogWarning("Zip bomb detection self-test failed.");
        }
        else
        {
            logger.LogInformation("Zip bomb detection self-test passed.");
        }

        if (!await EnsureZipTraversalDetectionAsync(scanner, cancellationToken))
        {
            logger.LogWarning("Zip traversal detection self-test failed.");
        }
        else
        {
            logger.LogInformation("Zip traversal detection self-test passed.");
        }
    }

    private static async Task<bool> EnsureZipBombDetectionAsync(IFileScanner scanner, CancellationToken cancellationToken)
    {
        using var archiveStream = new MemoryStream();
        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(ZipBombEntryName, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            var payload = new byte[ZipBombPayloadSize];
            Array.Fill(payload, (byte)'A');
            await entryStream.WriteAsync(payload.AsMemory(0, payload.Length), cancellationToken);
        }

        archiveStream.Seek(0, SeekOrigin.Begin);
        var isSafe = await scanner.IsSafeAsync(archiveStream, cancellationToken);
        return !isSafe;
    }

    private static async Task<bool> EnsureZipTraversalDetectionAsync(IFileScanner scanner, CancellationToken cancellationToken)
    {
        using var archiveStream = new MemoryStream();
        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("../evil.txt", CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            var payload = Encoding.UTF8.GetBytes("malicious");
            await entryStream.WriteAsync(payload.AsMemory(0, payload.Length), cancellationToken);
        }

        archiveStream.Seek(0, SeekOrigin.Begin);
        var isSafe = await scanner.IsSafeAsync(archiveStream, cancellationToken);
        return !isSafe;
    }
}

