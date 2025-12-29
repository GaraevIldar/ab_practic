#nullable enable
namespace PracticalWork.Library.Contracts.v1.Pagination;

/// <summary>
/// Запрос на выдачу книги
/// </summary>
/// <param name="PageNumber">номер страницы для вывода</param>
/// <param name="PageSize">количество логов на 1 странице</param>
/// <param name="EventDateFrom"> фильтр по времени отсчета</param>
/// <param name="EventDateTo">фильтр по времени окончания</param>
/// <param name="EventType"> фильтр по типу события</param>
public sealed record ActivityLogsPaginationRequest(
    int PageNumber = 1, 
    int PageSize = 20,
    string[]? EventType = null,
    DateOnly? EventDateFrom = null, 
    DateOnly? EventDateTo = null
);