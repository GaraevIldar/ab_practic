namespace PracticalWork.Library.Contracts.v1.Books.Response;

/// <summary>
/// Ответ на выдачи книги 
/// </summary>
/// <param name="Id">Идентификатор книги</param>
public sealed record BorrowBookResponse(Guid Id);