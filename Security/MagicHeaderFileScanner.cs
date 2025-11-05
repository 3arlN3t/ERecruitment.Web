using Microsoft.AspNetCore.Http;

namespace ERecruitment.Web.Security;

public class MagicHeaderFileScanner : IFileScanner
{
    private static readonly byte[] Pdf = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
    private static readonly byte[] Doc = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }; // OLE
    private static readonly byte[] Docx = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // ZIP

    public async Task<bool> IsSafeAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        return await IsSafeAsync(stream, cancellationToken);
    }

    public async Task<bool> IsSafeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var header = new byte[4];
        var read = await stream.ReadAsync(header.AsMemory(0, 4), cancellationToken);
        if (read < 4)
        {
            return false;
        }

        if (header.SequenceEqual(Pdf) || header.SequenceEqual(Doc) || header.SequenceEqual(Docx))
        {
            return true;
        }
        return false;
    }
}


