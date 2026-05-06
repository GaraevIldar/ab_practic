using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Data.PostgreSql.Extensions.Mappers;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly AppDbContext _appDbContext;

    public LibraryRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Borrow> GetBookBorrow(Guid bookId)
    {
        var borrow = await _appDbContext.BookBorrows
            .AsNoTracking()
            .FirstAsync(b => b.BookId == bookId);
        return borrow.ToBookBorrow();
    }

    public async Task<Borrow> BorrowBook(Guid bookId, Guid readerId)
    {
        var book = await _appDbContext.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        var reader = await _appDbContext.Readers.FirstOrDefaultAsync(r => r.Id == readerId);

        var borrow = new BookBorrowEntity
        {
            BookId = book.Id,
            ReaderId = reader.Id,
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30),
            Status = BookIssueStatus.Issued,
        };

        book.Status = BookStatus.Borrow;

        _appDbContext.Books.Update(book);
        _appDbContext.BookBorrows.Add(borrow);

        await _appDbContext.SaveChangesAsync();
        return borrow.ToBookBorrow();
    }

    public async Task<Guid> ReturnBook(Guid bookId)
    {
        var borrow = await _appDbContext.BookBorrows.FirstOrDefaultAsync(b => b.BookId == bookId);

        borrow.Status = BookIssueStatus.Returned;
        borrow.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);

        await _appDbContext.SaveChangesAsync();
        return borrow.BookId;
    }

    public async Task<IReadOnlyList<BorrowWithDetails>> GetBorrowsDueSoon(int daysAhead)
    {
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysAhead));

        var result = await _appDbContext.BookBorrows
            .AsNoTracking()
            .Where(b => b.Status == BookIssueStatus.Issued && b.DueDate == dueDate)
            .Join(_appDbContext.Books,
                b => b.BookId,
                book => book.Id,
                (b, book) => new { Borrow = b, Book = book })
            .Join(_appDbContext.Readers,
                x => x.Borrow.ReaderId,
                reader => reader.Id,
                (x, reader) => new BorrowWithDetails
                {
                    BorrowId = x.Borrow.Id,
                    BookId = x.Borrow.BookId,
                    BookTitle = x.Book.Title,
                    BookAuthors = x.Book.Authors.ToArray(),
                    ReaderId = x.Borrow.ReaderId,
                    ReaderFullName = reader.FullName,
                    ReaderEmail = reader.Email,
                    BorrowDate = x.Borrow.BorrowDate,
                    DueDate = x.Borrow.DueDate,
                })
            .ToListAsync();

        return result;
    }

    public async Task<WeeklyStats> GetWeeklyStats(DateTime from, DateTime to)
    {
        var fromDate = DateOnly.FromDateTime(from);
        var toDate = DateOnly.FromDateTime(to);

        var newBooks = await _appDbContext.Books
            .CountAsync(b => b.CreatedAt >= from && b.CreatedAt < to);

        var newReaders = await _appDbContext.Readers
            .CountAsync(r => r.CreatedAt >= from && r.CreatedAt < to);

        var borrowed = await _appDbContext.BookBorrows
            .CountAsync(b => b.BorrowDate >= fromDate && b.BorrowDate < toDate);

        var returned = await _appDbContext.BookBorrows
            .CountAsync(b => b.ReturnDate >= fromDate && b.ReturnDate < toDate
                             && b.Status == BookIssueStatus.Returned);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdue = await _appDbContext.BookBorrows
            .CountAsync(b => b.Status == BookIssueStatus.Issued && b.DueDate < today);

        return new WeeklyStats
        {
            PeriodFrom = from,
            PeriodTo = to,
            NewBooks = newBooks,
            NewReaders = newReaders,
            BooksBorrowed = borrowed,
            BooksReturned = returned,
            OverdueBorrows = overdue,
        };
    }

    public async Task<bool> HasNotificationBeenSent(Guid borrowId, string notificationType, TimeSpan window)
    {
        var cutoff = DateTime.UtcNow - window;
        return await _appDbContext.NotificationLogs
            .AnyAsync(n => n.BorrowId == borrowId
                           && n.NotificationType == notificationType
                           && n.SentAt >= cutoff);
    }

    public async Task RecordNotification(Guid borrowId, string notificationType)
    {
        _appDbContext.NotificationLogs.Add(new NotificationLogEntity
        {
            BorrowId = borrowId,
            NotificationType = notificationType,
            SentAt = DateTime.UtcNow,
        });
        await _appDbContext.SaveChangesAsync();
    }
}
