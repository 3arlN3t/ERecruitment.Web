using Microsoft.EntityFrameworkCore;

namespace ERecruitment.Web.Services;

public static class QueryExtensions
{
    /// <summary>
    /// Converts an IQueryable to a paginated result with a single optimized query.
    /// Uses Count() and Skip().Take() - executes 2 database queries.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Safety limit

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
