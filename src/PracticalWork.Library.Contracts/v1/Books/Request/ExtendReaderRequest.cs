namespace PracticalWork.Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на продление срока действия читательского билета
/// </summary>
/// <param name="NewExpiryDate">Новая дата окончания действия</param>
public sealed record ExtendReaderRequest(DateOnly NewExpiryDate);