using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Contracts.v1.Enums;

namespace PracticalWork.Library.Abstractions.Services;

/// <summary>
/// Сервис операций библиотеки: выдача и возврат книг
/// </summary>
public interface ILibraryService
{
    /// <summary>
    /// Выдача книги читателю
    /// </summary>
    Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId);

    /// <summary>
    /// Получение постраничного списка книг, доступных для выдачи (не в архиве)
    /// </summary>
    Task<BookListResponse> GetBooksNoArchive(int pageNumber, int pageSize, BookCategory? category, string author);

    /// <summary>
    /// Возврат книги читателем
    /// </summary>
    Task<ReturnBookResponse> ReturnBook(Guid bookId);

    /// <summary>
    /// Получение детальной информации о книге по идентификатору или названию
    /// </summary>
    Task<BookDetailsResponse> GetBookDetailsAsync(string idOrTitle);
}