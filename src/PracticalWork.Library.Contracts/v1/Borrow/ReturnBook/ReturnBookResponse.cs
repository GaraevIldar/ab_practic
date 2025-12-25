namespace PracticalWork.Library.Contracts.v1.Books.Response;

/// <summary>
/// Ответ на возвращение книги 
/// </summary>
/// <param name="Id">Идентификатор книги</param>
public sealed record ReturnBookResponse(Guid Id);