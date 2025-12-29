#nullable enable
using PracticalWork.Library.Contracts.v1.Enums;

namespace PracticalWork.Library.Contracts.v1.Pagination;

/// <summary>
/// Запрос на выдачу книги
/// </summary>
/// <param name="PageNumber">номер страницы для вывода</param>
/// <param name="PageSize">количество книг на 1 странице</param>
/// <param name="Category">фильтр по категории</param>
/// <param name="Author"> фильтр по автору</param>

public sealed record PaginationNoArchivedBookRequest(
    int PageNumber, 
    int PageSize, 
    BookCategory? Category = null,    
    string? Author = null  
);