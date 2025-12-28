
#nullable enable
using System.Runtime.CompilerServices;
using PracticalWork.Library.Contracts.v1.Enums;

namespace PracticalWork.Library.Contracts.v1.Pagination;
/// <summary>
/// Запрос на выдачу книги
/// </summary>
/// <param name="PageNumber">номер страницы для вывода</param>
/// <param name="PageSize">количество книг на 1 странице</param>
/// <param name="Status"> фильтр по статусу</param>
/// <param name="Category">фильтр по категории</param>
/// <param name="Author"> фильтр по автору</param>

public sealed record PaginationBookRequest(
    int PageNumber, 
    int PageSize, 
    BookStatus? Status = null,        // опционально
    BookCategory? Category = null,    // опционально
    string? Author = null  
    );