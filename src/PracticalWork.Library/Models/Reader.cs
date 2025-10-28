namespace PracticalWork.Library.Models;

using System;
using PracticalWork.Library.Enums;

/// <summary>
/// Читатель
/// </summary>
public sealed class Reader
{
    public Guid Id { get; set; }

    /// <summary>ФИО читателя</summary>
    public string FullName { get; set; }

    /// <summary>Номер телефона (уникальный)</summary>
    public string PhoneNumber { get; set; }

    /// <summary>Дата окончания действия читательского билета</summary>
    public DateOnly ExpiryDate { get; set; }

    /// <summary>Активность карточки</summary>
    public bool IsActive { get; set; }

    /// <summary>Дата создания</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Дата обновления</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Проверка, истёк ли срок действия карточки</summary>
    public bool IsExpired() => ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Деактивация читателя (например, при утере карточки)</summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>Продление действия карточки</summary>
    /// <param name="newExpiryDate">Новая дата окончания</param>
    public void ExtendExpiry(DateOnly newExpiryDate)
    {
        if (newExpiryDate <= ExpiryDate)
            throw new InvalidOperationException("Новая дата окончания должна быть позже текущей.");
        ExpiryDate = newExpiryDate;
    }
}
