namespace PracticalWork.Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на создание читателя
/// </summary>
/// <param name="FullName">ФИО читателя</param>
/// <param name="PhoneNumber">Номер телефона (уникальный)</param>
/// <param name="ExpiryDate">Дата окончания действия читательского билета</param>
public sealed record CreateReaderRequest(string FullName, string PhoneNumber, DateOnly ExpiryDate);