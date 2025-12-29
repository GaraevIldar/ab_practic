using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IActivityLogPaginationService
{
    IReadOnlyList<ActivityLog> PaginationLogs(
        IReadOnlyList<ActivityLog> source,
        int pageNumber,
        int pageSize);
}