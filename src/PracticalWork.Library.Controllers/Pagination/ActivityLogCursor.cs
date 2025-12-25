using PracticalWork.Library.Models;

namespace PracticalWork.Library.Controllers.Pagination;

/// <summary>
/// Курсор пагинации логов активности
/// </summary>
public record ActivityLogCursor(
    DateTime EventDate,
    string EventType
) : Cursor;