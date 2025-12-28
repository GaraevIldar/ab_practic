using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Contracts.v1.Pagination;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Contracts.v1.Enums;
using PracticalWork.Library.Exceptions.Book;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICacheService _cacheService;
    private readonly IMinioService _minioService;
    private readonly IBookPaginationService _paginationService;
    private readonly string _cacheVersion;
    private readonly string _booksListPrefix;
    private readonly double _cacheTtlInMinutes;

    public BookService(IBookRepository bookRepository,
        ICacheService cacheService,
        IMinioService minioService,
        IBookPaginationService paginationService,
        IConfiguration configuration)
    {
        _bookRepository = bookRepository;
        _cacheService = cacheService;
        _minioService = minioService;
        _paginationService = paginationService;
        var section = configuration.GetSection("App:Redis:Books");
        _cacheVersion = section["VersionKey"];
        _booksListPrefix = section["BooksList:Prefix"];
        _cacheTtlInMinutes = section.GetValue<double>("BooksList:TtlInMinutes");
    }

    public async Task<Guid> CreateBook(Book book)
    {
        book.Status = Enums.BookStatus.Available;
        
        var id = await _bookRepository.CreateBook(book);

        await _cacheService.InvalidateCache(_cacheVersion);

        return id;
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        var existingEntity = await _bookRepository.GetBookById(id);
        
        if (existingEntity == null)
            throw new BookNotFoundException(id);

        if (existingEntity.Status == Enums.BookStatus.Archived)
            throw new BookArchivedException(id);
        
        await _bookRepository.UpdateBook(id, book);
        await _cacheService.InvalidateCache(_cacheVersion);
    }
    
    public async Task MoveToArchive(Guid id)
    {
        var book = await _bookRepository.GetBookById(id);
        
        if (book == null)
            throw new BookNotFoundException(id);

        if (book.Status == Enums.BookStatus.Archived)
            throw new BookAlreadyArchivedException();
        
        if (book.Status == Enums.BookStatus.Borrow)
            throw new BookBorrowedException();
            
        await _bookRepository.MoveToArchive(id);
        await _cacheService.InvalidateCache(_cacheVersion);
    }
    
    public async Task<BookListResponse> GetBooks(int pageNumber, int pageSize, BookStatus? status, BookCategory? category, string author)
    {
        var cacheVersion = await _cacheService.GetCurrentCacheVersion(_cacheVersion);
        var prms = new
        {
            category, 
            author, 
            status
        };
        var cacheKey = _cacheService.GenerateCacheKey(_booksListPrefix, cacheVersion, prms);
        var cached = await _cacheService.GetAsync<BookListResponse>(cacheKey);
        if (cached != null)
            return cached;

        var books = await _bookRepository.GetFilterBooks(status, author);

        var paginationBooks = _paginationService.PaginationBooks(books, pageNumber, pageSize);
    
        await _cacheService.SetAsync(
            cacheKey,
            paginationBooks,
            TimeSpan.FromMinutes(_cacheTtlInMinutes));

        return paginationBooks;
    }
    public async Task AddBookDetails(Guid bookId, string description, IFormFile coverFile)
    {
        var book = await _bookRepository.GetBookById(bookId);

        if (book == null)
            throw new BookNotFoundException(bookId);

        var ext = Path.GetExtension(coverFile.FileName).ToLower();
        var now = DateTime.UtcNow;

        string objectName = $"book-covers/{now:yyyy}/{now:MM}/{bookId}{ext}";

        using var stream = coverFile.OpenReadStream();
        var path = await _minioService.UploadFileAsync(
            objectName,
            stream,
            coverFile.ContentType);

        book.Description = description;
        book.CoverImagePath = path;

        await _bookRepository.UpdateBook(bookId, book);
        await _cacheService.InvalidateCache(_cacheVersion);
    }
}