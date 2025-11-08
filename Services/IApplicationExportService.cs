using System;
using System.Threading;
using System.Threading.Tasks;
using ERecruitment.Web.Models;

namespace ERecruitment.Web.Services;

public interface IApplicationExportService
{
    Task<FileDownloadResult?> ExportMasterListAsync(Guid jobId, string scope, CancellationToken cancellationToken = default);
    Task<FileDownloadResult> ExportApplicationsCsvAsync(string? search = null, ApplicationStatus? status = null, Guid? jobId = null, CancellationToken cancellationToken = default);
}

