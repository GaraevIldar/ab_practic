namespace PracticalWork.Library.Events;

/// <summary>
/// Событие закрытия карточки читателя
/// </summary>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="FullName">Полное имя читателя</param>
/// <param name="ClosedAt">Дата и время закрытия карточки</param>
/// <param name="Reason">Причина закрытия карточки</param>
/// <param name="OccurredOn">Дата и время возникновения события</param>
public sealed record ReaderClosedEvent(
    Guid ReaderId,
    string FullName,
    DateTime ClosedAt,
    string Reason,
#pragma warning disable CS8907 // Parameter is unread. Did you forget to use it to initialize the property with that name?
    DateTime OccurredOn = default
) : BaseLibraryEvent("reader.closed")
{
    /// <summary>
    /// Основной конструктор с автогенерацией идентификаторов события
    /// </summary>
    public ReaderClosedEvent(Guid readerId, string fullName, DateTime closedAt, string reason)
        : this(ReaderId: readerId, FullName: fullName, ClosedAt: closedAt, Reason: reason, OccurredOn: DateTime.UtcNow){}
}