using Microsoft.Extensions.Configuration;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Book;
using PracticalWork.Library.Exceptions.Library;
using PracticalWork.Library.Exceptions.Reader;
using BookCategory = PracticalWork.Library.Contracts.v1.Enums.BookCategory;
using BookStatus = PracticalWork.Library.Contracts.v1.Enums.BookStatus;

namespace PracticalWork.Library.Services;

public sealed class LibraryService : ILibraryService
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IReaderRepository _readerRepository;
    private readonly IMinioService _minioService;
    private readonly ICacheService _cacheService;
    private readonly IBookPaginationService _paginationService;
    private readonly string _libraryCacheVersion;
    private readonly string _libraryBooksListPrefix;
    private readonly double _libraryBooksTtlInMinutes;
    private readonly string _booksDetailsPrefix;
    private readonly double _booksDetailsTtlInMinutes;

    public LibraryService(
        ILibraryRepository libraryRepository,
        IBookRepository bookRepository,
        IReaderRepository readerRepository,
        IMinioService minioService,
        ICacheService cacheService,
        IBookPaginationService paginationService,
        IConfiguration configuration)
    {
        _libraryRepository = libraryRepository;
        _bookRepository = bookRepository;
        _readerRepository = readerRepository;
        _minioService = minioService;
        _cacheService = cacheService;
        _paginationService = paginationService;
        var section = configuration.GetSection("App:Redis:Books");
        _libraryCacheVersion = section["VersionKey"];
        _libraryBooksListPrefix = section["LibraryBooks:Prefix"];
        _libraryBooksTtlInMinutes = section.GetValue<double>("LibraryBooks:TtlInMinutes");
        _booksDetailsPrefix = section["BookDetails:Prefix"];
        _booksDetailsTtlInMinutes = section.GetValue<double>("BookDetails:TtlInMinutes");
    }

    public async Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId)
    {
        if (!await _readerRepository.IsReaderExist(readerId))
            throw new ReaderNotFoundException(readerId);

        if (!await _bookRepository.IsBookExist(bookId))
            throw new BookNotFoundException(bookId);

        var book = await _bookRepository.GetBookById(bookId);

        if (book.Status != Enums.BookStatus.Available)
            throw new BookNotAvailableException();

        try
        {
            var borrowId = await _libraryRepository.BorrowBook(bookId, readerId);
            return new BorrowBookResponse(borrowId);
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при выдаче книги", ex);
        }
    }
    
    public async Task<BookListResponse> GetBooksNoArchive(int pageNumber, int pageSize, BookStatus? status, BookCategory? category, string author)
    {
        try
        {
            var cacheVersion = await _cacheService.GetCurrentCacheVersion(_libraryCacheVersion);
            var prms = new
            {
                category, 
                author, 
                status
            };
            var cacheKey = _cacheService.GenerateCacheKey(_libraryBooksListPrefix, cacheVersion, prms);
            var cached = await _cacheService.GetAsync<BookListResponse>(cacheKey);
            if (cached != null)
                return cached;

            var books = await _bookRepository.GetBooksNoArchive(status, author);

            await _cacheService.SetAsync(cacheKey, books, TimeSpan.FromMinutes(_libraryBooksTtlInMinutes));

            return _paginationService.PaginationBooks(books, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при получении списка книг", ex);
        }
    }

    public async Task<ReturnBookResponse> ReturnBook(Guid bookId)
    {
        var borrow = await _libraryRepository.GetBookBorrow(bookId);

        if (borrow == null)
            throw new BookBorrowNotFoundException(bookId);

        try
        {
            var returnId = await _libraryRepository.ReturnBook(bookId);
            return new ReturnBookResponse(returnId);
        }
        catch (Exception ex)
        {
            throw new LibraryServiceException("Ошибка при возврате книги", ex);
        }
    }

    public async Task<BookDetailsResponse> GetBookDetailsAsync(string idOrTitle)
    {
        Guid.TryParse(idOrTitle, out var bookId);

        var book = bookId != Guid.Empty
            ? await _bookRepository.GetBookById(bookId)
            : await _bookRepository.GetBookByTitle(idOrTitle);

        if (book == null)
            throw new BookNotFoundException(book.Id);

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