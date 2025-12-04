using JetBrains.Annotations;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IBookRepository
{
    Task<Guid> CreateBook(Book book);
    Task UpdateBook(Guid id, Book book);
    Task<Book> GetBookById(Guid id);
    Task  MoveToArchive(Guid id);
    Task<BookListResponse> GetBooks();
    Task<bool> IsBookExist(Guid id);
    Task<BookListResponse> GetBooksNoArchive();
    Task<Book> GetBookByTitle(string title);
    Task UpdateBookDetailsAsync(Guid bookId, string description, string coverPath);
}