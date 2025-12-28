using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Contracts.v1.Enums;

namespace PracticalWork.Library.Abstractions.Services;

public interface ILibraryService
{
    Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId);
    Task<BookListResponse> GetBooksNoArchive(int pageNumber, int pageSize, BookStatus? status, BookCategory? category, string author);
    Task<ReturnBookResponse> ReturnBook(Guid bookId);

    Task<BookDetailsResponse> GetBookDetailsAsync(string idOrTitle);
}