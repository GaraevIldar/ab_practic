using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface ILibraryRepository
{
    Task<Borrow> BorrowBook(Guid bookId, Guid readerId);
    Task<Guid> ReturnBook(Guid bookId);
    Task<Borrow> GetBookBorrow(Guid bookId);
}