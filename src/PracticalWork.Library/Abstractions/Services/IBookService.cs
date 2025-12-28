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

    Task UpdateBook(Guid id, Book book);

    Task MoveToArchive(Guid id);
    Task<BookListResponse> GetBooks(int pageNumber, int pageSize, BookStatus? status, BookCategory? category, string author);
    Task AddBookDetails(Guid bookId, string description, IFormFile coverFile);
}