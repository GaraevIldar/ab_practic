namespace PracticalWork.Library.Contracts.v1.Books.Response;

/// <summary>
/// Запрос на закрытие карточки
/// </summary>
/// <param name="IdReader"> Id читателя</param>
public sealed record CloseReaderCardResponse(Guid IdReader);