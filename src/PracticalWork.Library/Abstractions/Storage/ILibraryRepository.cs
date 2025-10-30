using PracticalWork.Library.Contracts.v1.Books.Response;

namespace PracticalWork.Library.Abstractions.Storage;

public interface ILibraryRepository
{
    Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId);
    Task<ReturnBookResponse> ReturnBook(Guid bookId);
}