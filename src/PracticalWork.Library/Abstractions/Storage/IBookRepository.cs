using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IBookRepository
{
    Task<Guid> CreateBook(Book book);
    Task<Guid> UpdateBook(Guid id, Book book);
    Task <ArchiveBookResponse> MoveToArchive(Guid id);
    Task<BookListResponse> GetBooks();

    Task<bool> IsBookExist(Guid id);
    Task<BookListResponse> GetBooksNoArchive();
    // Task<BookDetailsResponse> AddDetails(AddBookDetailsRequest details);
}