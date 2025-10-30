using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Exceptions;

namespace PracticalWork.Library.Services;

public class LibraryService : ILibraryService
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IBookRepository _bookRepository;

    public LibraryService(ILibraryRepository libraryRepository,
        IBookRepository bookRepository)
    {
        _libraryRepository = libraryRepository;
        _bookRepository = bookRepository;
    }

    public async Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId)
    {
        try
        {
            return await _libraryRepository.BorrowBook(bookId, readerId);
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при выдачи книги", ex);
        }
    }

    public async Task<BookListResponse> GetBooksNoArchive()
    {
        try
        {
            return await _bookRepository.GetBooksNoArchive();
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при получении книг", ex);
        }
    }

    public async Task<ReturnBookResponse> ReturnBook(Guid bookId)
    {
        try
        {
            return await _libraryRepository.ReturnBook(bookId);
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при возвращении книги", ex);
        }
    }
}