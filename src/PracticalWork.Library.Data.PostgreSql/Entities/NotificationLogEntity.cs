namespace PracticalWork.Library.Data.PostgreSql.Entities;

/// <summary>
/// Журнал отправленных уведомлений — используется для дедупликации
/// </summary>
public sealed class NotificationLogEntity
{
    public long Id { get; set; }
    public Guid BorrowId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
