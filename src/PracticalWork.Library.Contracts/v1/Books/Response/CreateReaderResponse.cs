namespace PracticalWork.Library.Contracts.v1.Books.Response;

/// <summary>
/// Ответ на создание читателя
/// </summary>
/// <param name="Id">Идентификатор читателя</param>
public sealed record CreateReaderResponse(Guid Id);