using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Data.PostgreSql.Extensions.Mappers;
using PracticalWork.Library.Contracts.v1.Enums;
using PracticalWork.Library.Models;


namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly AppDbContext _appDbContext;

    public BookRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Guid> CreateBook(Book book)
    {
        AbstractBookEntity entity = book.Category switch
        {
            BookCategory.ScientificBook => new ScientificBookEntity(),
            BookCategory.EducationalBook => new EducationalBookEntity(),
            BookCategory.FictionBook => new FictionBookEntity(),
            _ => throw new ArgumentException($"Неподдерживаемая категория книги: {book.Category}",
                nameof(book.Category))
        };

        entity.Title = book.Title;
        entity.Description = book.Description;
        entity.Year = book.Year;
        entity.Authors = book.Authors;
        entity.Status = book.Status;

        _appDbContext.Add(entity);
        await _appDbContext.SaveChangesAsync();

        return entity.Id;
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        var entity = await _appDbContext.Books.FindAsync(id);

        entity.Title = book.Title;
        entity.Description = book.Description;
        entity.Year = book.Year;
        entity.Authors = book.Authors;
        entity.Status = book.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        if (book.CoverImagePath != null)
        {
            entity.CoverImagePath = book.CoverImagePath;
        }

        _appDbContext.Update(entity);
        

        await _appDbContext.SaveChangesAsync();
    }

    public async Task<Book> GetBookById(Guid id)
    {
        var book = await _appDbContext.Books
            .FindAsync(id);
        return book?.ToBook();
    }
    public async Task MoveToArchive(Guid id)
    {
        var book = await _appDbContext.Books.FindAsync(id);
        
        book.Status = (Enums.BookStatus)BookStatus.Archived;
        
        _appDbContext.Update(book);
        
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<BookListResponse> GetBooks()
    {
        var books = await _appDbContext.Books
            .AsNoTracking()   
            .OrderBy(b => b.Status)
            .ToListAsync();

        return new BookListResponse()
        {
            TotalCount = books.Count,
            Books = books.Select(b => new BookItemResponse
            {
                Id = b.Id,
                Title = b.Title,
                Authors = b.Authors,
                Description = b.Description,
                Year = b.Year,
                Status = b.Status.ToString(),
                CoverImagePath = b.CoverImagePath,
                Category = b switch
                {
                    FictionBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.FictionBook,
                    EducationalBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.EducationalBook,
                    ScientificBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.ScientificBook,
                    _ => (Contracts.v1.Enums.BookCategory)BookCategory.Default
                }
            }).ToList()
        };
    }
    public async Task<BookListResponse> GetBooksNoArchive()
    {
        var books = await _appDbContext.Books
            .AsNoTracking()
            .Where(b=> b.Status != Enums.BookStatus.Archived)
            .OrderBy(b => b.Status)
            .ToListAsync();

        var response = new BookListResponse()
        {
            TotalCount = books.Count,
            Books = books.Select(b => new BookItemResponse
            {
                Id = b.Id,
                Title = b.Title,
                Authors = b.Authors,
                Description = b.Description,
                Year = b.Year,
                Status = b.Status.ToString(),
                CoverImagePath = b.CoverImagePath,
                Category = b switch
                {
                    FictionBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.FictionBook,
                    EducationalBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.EducationalBook,
                    ScientificBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.ScientificBook,
                    _ => (Contracts.v1.Enums.BookCategory)BookCategory.Default
                }
            }).ToList()

        };
        

        return response;
    }

    public async Task<bool> IsBookExist(Guid id)
    {
        var book = await _appDbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (book == null) return false;

        return true;
    }
    public async Task<Book> GetBookByTitle(string title)
    {
        var entity = await _appDbContext.Books
            .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower());

        return entity?.ToBook();
    }

    public async Task UpdateBookDetailsAsync(Guid bookId, string description, string coverPath)
    {
        var book = await GetBookById(bookId);

        book.Description = description;
        book.CoverPath = coverPath;

        await UpdateBook(bookId, book);
    }
}