namespace PracticalWork.Library.Models;

/// <summary>
/// Выдача книги с деталями о читателе и книге (для уведомлений)
/// </summary>
public sealed class BorrowWithDetails
{
    public Guid BorrowId { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; }
    public string[] BookAuthors { get; set; }
    public Guid ReaderId { get; set; }
    public string ReaderFullName { get; set; }
    public string ReaderEmail { get; set; }
    public DateOnly BorrowDate { get; set; }
    public DateOnly DueDate { get; set; }
}
