namespace PracticalWork.Library.Contracts.v1.Books.Request;
/// <summary>
/// Запрос на закрытие карточки
/// </summary>
/// <param name="IdReader"> Id читателя</param>
public sealed record CloseReaderCardRequest(Guid IdReader);