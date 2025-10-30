using PracticalWork.Library.Contracts.v1.Books.Response;

namespace PracticalWork.Library.Abstractions.Services;

public interface ILibraryService
{
    Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId);
    Task<BookListResponse> GetBooksNoArchive();
    Task<ReturnBookResponse> ReturnBook(Guid bookId);
    // Task<BookDetailsResponse> GetBookDetails(string idOrTitle);
}