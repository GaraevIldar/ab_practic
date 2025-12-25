namespace PracticalWork.Library.Enums;

public enum ReportStatus
{
    /// <summary>
    /// В процессе
    /// </summary>
    InProgress = 0,
    /// <summary>
    /// Готов
    /// </summary>
    Generated = 10,
    /// <summary>
    /// Генерация с ошибкой
    /// </summary>
    Error = 20,
}