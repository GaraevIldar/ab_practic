using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class ActivityLogPaginationService: IActivityLogPaginationService
{
    public IReadOnlyList<ActivityLog> PaginationLogs(IReadOnlyList<ActivityLog> source, int pageNumber, int pageSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var pagedBooks = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedBooks;
    }
}