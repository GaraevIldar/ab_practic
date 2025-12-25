namespace PracticalWork.Library.Events;

/// <summary>
/// Событие выдачи книги читателю
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="BookTitle">Название книги</param>
/// <param name="ReaderName">ФИО читателя</param>
/// <param name="BorrowDate">Дата выдачи книги</param>
/// <param name="DueDate">Срок возврата книги</param>
/// <param name="OccurredOn">Дата и время возникновения события</param>
public sealed record BookBorrowedEvent(
    Guid BookId,
    Guid ReaderId,
    string BookTitle,
    string ReaderName,
    DateTime BorrowDate,
    DateTime DueDate,
    DateTime OccurredOn = default
) : BaseLibraryEvent("book.borrowed")
{
    /// <summary>
    /// Основной конструктор с автогенерацией идентификаторов события
    /// </summary>
    public BookBorrowedEvent(Guid bookId, Guid readerId, string bookTitle, string readerName, DateTime borrowDate, DateTime dueDate)
        : this(BookId: bookId, ReaderId: readerId, BookTitle: bookTitle, ReaderName: readerName, BorrowDate: borrowDate, DueDate: dueDate, OccurredOn: DateTime.UtcNow)
    {
    }
}