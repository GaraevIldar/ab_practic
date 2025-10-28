using System;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Models;

/// <summary>
/// Выдача книги
/// </summary>
public sealed class Borrow
{
    public Guid Id { get; set; }

    /// <summary>Идентификатор книги</summary>
    public Guid BookId { get; set; }

    /// <summary>Идентификатор читателя</summary>
    public Guid ReaderId { get; set; }

    /// <summary>Дата выдачи книги</summary>
    public DateOnly BorrowDate { get; set; }

    /// <summary>Срок возврата книги</summary>
    public DateOnly DueDate { get; set; }

    /// <summary>Фактическая дата возврата</summary>
    public DateOnly? ReturnDate { get; set; }

    /// <summary>Статус выдачи (выдана, возвращена, возвращена с просрочкой)</summary>
    public BookIssueStatus Status { get; set; } = BookIssueStatus.Issued;

    /// <summary>Дата создания записи</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Дата обновления записи</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Проверка, просрочена ли выдача
    /// </summary>
    public bool IsOverdue() =>
        Status == BookIssueStatus.Issued && DateOnly.FromDateTime(DateTime.UtcNow) > DueDate;

    /// <summary>
    /// Отметить книгу как возвращённую
    /// </summary>
    public void MarkAsReturned()
    {
        if (Status != BookIssueStatus.Issued)
            throw new InvalidOperationException("Можно вернуть только выданную книгу.");

        ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        Status = IsOverdue() ? BookIssueStatus.Overdue : BookIssueStatus.Returned;
    }

    /// <summary>
    /// Проверка и обновление статуса при просрочке
    /// </summary>
    public void CheckAndMarkOverdue()
    {
        if (IsOverdue())
            Status = BookIssueStatus.Overdue;
    }
}