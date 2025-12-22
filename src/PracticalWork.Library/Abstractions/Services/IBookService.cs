using Microsoft.AspNetCore.Http;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IBookService
{
    /// <summary>
    /// Создание книги
    /// </summary>
    Task<Guid> CreateBook(Book book);

    Task UpdateBook(Guid id, Book book);

    Task MoveToArchive(Guid id);
    Task<BookListResponse> GetBooks();
    Task AddBookDetails(Guid bookId, string description, IFormFile coverFile);
}