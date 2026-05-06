using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface ILibraryRepository
{
    Task<Borrow> BorrowBook(Guid bookId, Guid readerId);
    Task<Guid> ReturnBook(Guid bookId);
    Task<Borrow> GetBookBorrow(Guid bookId);
    Task<IReadOnlyList<BorrowWithDetails>> GetBorrowsDueSoon(int daysAhead);
    Task<WeeklyStats> GetWeeklyStats(DateTime from, DateTime to);
    Task<bool> HasNotificationBeenSent(Guid borrowId, string notificationType, TimeSpan window);
    Task RecordNotification(Guid borrowId, string notificationType);
}