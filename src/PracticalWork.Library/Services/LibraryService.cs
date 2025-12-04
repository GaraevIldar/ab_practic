using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using BookStatus = PracticalWork.Library.Contracts.v1.Enums.BookStatus;

namespace PracticalWork.Library.Services;

public class LibraryService : ILibraryService
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IReaderRepository _readerRepository;
    private readonly IMinioService _minioService;
    private readonly ICacheService _cacheService;
    
    private const string BooksCacheKey = "books:no-archive";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

    public LibraryService(ILibraryRepository libraryRepository,
        IBookRepository bookRepository
        , IReaderRepository readerRepository
        , IMinioService minioService,
        ICacheService cacheService)
    {
        _libraryRepository = libraryRepository;
        _bookRepository = bookRepository;
        _readerRepository = readerRepository;
        _minioService = minioService;
        _cacheService = cacheService;
    }

    public async Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId)
    {
        var readerExists = await _readerRepository.IsReaderExist(readerId);
        var bookExists = await _bookRepository.IsBookExist(bookId);

        if (!readerExists)
            throw new ReaderServiceException("Читатель не найден");
        
        if (!bookExists)
            throw new BookServiceException("Книга не найдена");
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
            var cached = await _cacheService.GetAsync<BookListResponse>(BooksCacheKey);
            if (cached != null)
                return cached;
            
            var booksResponse = await _bookRepository.GetBooksNoArchive();
            
            await _cacheService.SetAsync(BooksCacheKey, booksResponse, CacheExpiry);
            
            return booksResponse;
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при получении книг", ex);
        }
    }

    public async Task<ReturnBookResponse> ReturnBook(Guid bookId)
    {
        var borrow= await _libraryRepository.GetBookBorrow(bookId);

        if (borrow == null )   
            throw new InvalidOperationException("Нет записи о выдачи");
        try
        {
            return await _libraryRepository.ReturnBook(bookId);
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при возвращении книги", ex);
        }
    }
    public async Task<BookDetailsResponse> GetBookDetailsAsync(string idOrTitle)
    {
        Guid.TryParse(idOrTitle, out var bookId);
        
        var book = bookId != Guid.Empty
            ? await _bookRepository.GetBookById(bookId)
            : await _bookRepository.GetBookByTitle(idOrTitle);

        if (book == null)
            throw new BookServiceException($"Книга с идентификатором или названием '{idOrTitle}' не найдена");

        string coverUrl = string.IsNullOrEmpty(book.CoverImagePath)
            ? string.Empty
            : $"https://{_minioService.Endpoint}/{book.CoverImagePath}";

        return new BookDetailsResponse(
            Id: book.Id,
            Title: book.Title,
            Category: book.Category,
            Authors: book.Authors,
            Description: book.Description,
            Year: book.Year,
            CoverImagePath: coverUrl,
            IsArchived: book.Status == Enums.BookStatus.Archived
        );
    }

}