namespace PracticalWork.Library.Models;

/// <summary>
/// Еженедельная статистика библиотеки
/// </summary>
public sealed class WeeklyStats
{
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public int NewBooks { get; set; }
    public int NewReaders { get; set; }
    public int BooksBorrowed { get; set; }
    public int BooksReturned { get; set; }
    public int OverdueBorrows { get; set; }
}
