namespace PracticalWork.Library.Events;

/// <summary>
/// Событие архивации книги в библиотеке
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="Title">Название книги</param>
/// <param name="Reason">Причина архивации</param>
/// <param name="ArchivedAt">Дата и время архивации</param>
/// <param name="OccurredOn">Дата и время возникновения события</param>
public sealed record BookArchivedEvent(
    Guid BookId,
    string Title,
    string Reason,
    DateTime ArchivedAt,
    DateTime OccurredOn = default
) : BaseLibraryEvent("book.archived")
{
    /// <summary>
    /// Основной конструктор с автогенерацией идентификаторов события
    /// </summary>
    public BookArchivedEvent(Guid bookId, string title, string reason, DateTime archivedAt)
        : this(BookId: bookId, Title: title, Reason: reason, ArchivedAt: archivedAt, OccurredOn: DateTime.UtcNow)
    {
    }
}