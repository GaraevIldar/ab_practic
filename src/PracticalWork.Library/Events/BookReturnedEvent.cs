namespace PracticalWork.Library.Events;

/// <summary>
/// Событие возврата книги в библиотеку
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="BookTitle">Название книги</param>
/// <param name="ReaderName">ФИО читателя</param>
/// <param name="ReturnDate">Дата возврата книги</param>
/// <param name="OccurredOn">Дата и время возникновения события</param>
public sealed record BookReturnedEvent(
    Guid BookId,
    Guid ReaderId,
    string BookTitle,
    string ReaderName,
    DateTime ReturnDate,
#pragma warning disable CS8907 // Parameter is unread. Did you forget to use it to initialize the property with that name?
    DateTime OccurredOn = default
) : BaseLibraryEvent("book.returned")
{
    /// <summary>
    /// Основной конструктор с автогенерацией идентификаторов события
    /// </summary>
    public BookReturnedEvent(Guid bookId, Guid readerId, string bookTitle, string readerName, DateTime returnDate)
        : this(BookId: bookId, ReaderId: readerId, BookTitle: bookTitle, ReaderName: readerName, ReturnDate: returnDate,
            OccurredOn: DateTime.UtcNow)
    {
    }
}