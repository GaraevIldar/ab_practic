using Microsoft.AspNetCore.Http;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Contracts.v1.Pagination;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;
using BookCategory = PracticalWork.Library.Contracts.v1.Enums.BookCategory;
using BookStatus = PracticalWork.Library.Contracts.v1.Enums.BookStatus;

namespace PracticalWork.Library.Abstractions.Services;

public interface IBookService
{
    /// <summary>
    /// Создание книги
    /// </summary>
    Task<Guid> CreateBook(Book book);

    /// <summary>
    /// Обновление данных книги
    /// </summary>
    Task UpdateBook(Guid id, Book book);

    /// <summary>
    /// Перемещение книги в архив
    /// </summary>
    Task MoveToArchive(Guid id);

    /// <summary>
    /// Получение постраничного списка книг с фильтрацией
    /// </summary>
    Task<BookListResponse> GetBooks(int pageNumber, int pageSize, BookStatus? status, BookCategory? category, string author);

    /// <summary>
    /// Добавление детальной информации и обложки к книге
    /// </summary>
    Task AddBookDetails(Guid bookId, string description, IFormFile coverFile);
}