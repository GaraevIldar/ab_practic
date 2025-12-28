namespace PracticalWork.Library.Events;

/// <summary>
/// Событие создания новой карточки читателя
/// </summary>
/// <param name="ReaderId">Уникальный идентификатор читателя</param>
/// <param name="FullName">Полное имя читателя</param>
/// <param name="PhoneNumber">Номер телефона читателя</param>
/// <param name="ExpiryDate">Дата окончания действия карточки</param>
/// <param name="CreatedAt">Дата и время создания карточки</param>
/// <param name="OccurredOn">Дата и время возникновения события</param>
public sealed record ReaderCreatedEvent(
    Guid ReaderId,
    string FullName,
    string PhoneNumber,
    DateTime ExpiryDate,
    DateTime CreatedAt,
#pragma warning disable CS8907 // Parameter is unread. Did you forget to use it to initialize the property with that name?
    DateTime OccurredOn = default
) : BaseLibraryEvent("reader.created")
{
    /// <summary>
    /// Основной конструктор с автогенерацией идентификаторов события
    /// </summary>
    public ReaderCreatedEvent(Guid readerId, string fullName, string phoneNumber, DateTime expiryDate, DateTime createdAt)
        : this(ReaderId: readerId, FullName: fullName, PhoneNumber: phoneNumber, ExpiryDate: expiryDate, CreatedAt: createdAt, OccurredOn: DateTime.UtcNow){}
}
