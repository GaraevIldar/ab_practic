using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IBookRepository
{
    Task<Guid> CreateBook(Book book);
    Task<Guid> UpdateBook(Guid id, Book book);
    Task <Guid> MoveToArchive(Guid id);
}