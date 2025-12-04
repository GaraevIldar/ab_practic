using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface ILibraryRepository
{
    Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId);
    Task<ReturnBookResponse> ReturnBook(Guid bookId);
    Task<Borrow> GetBookBorrow(Guid bookId);
}