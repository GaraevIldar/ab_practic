namespace PracticalWork.Library.Events;

/// <summary>
/// Событие закрытия карточки читателя
/// </summary>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="FullName">Полное имя читателя</param>
/// <param name="ClosedAt">Дата и время закрытия карточки</param>
public sealed record ReaderClosedEvent(
    Guid ReaderId,
    string FullName,
    DateTime ClosedAt
) : BaseLibraryEvent("reader.closed")
{
    /// <summary>
    /// Основной конструктор с автогенерацией идентификаторов события
    /// </summary>
    public ReaderClosedEvent(Guid readerId, string fullName, DateTime closedAt, string reason)
        : this(ReaderId: readerId, FullName: fullName, ClosedAt: closedAt){}
}