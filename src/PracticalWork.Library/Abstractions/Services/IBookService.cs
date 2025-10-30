using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IBookService
{
    /// <summary>
    /// Создание книги
    /// </summary>
    Task<Guid> CreateBook(Book book);

    Task<Guid> UpdateBook(Guid id, Book book);

    Task<ArchiveBookResponse> MoveToArchive(Guid id);
    Task<BookListResponse> GetBooks();
    // Task<BookDetailsResponse> AddDetails(AddBookDetailsRequest details);
}