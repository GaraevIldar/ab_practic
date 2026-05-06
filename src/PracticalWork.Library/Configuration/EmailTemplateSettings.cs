namespace PracticalWork.Library.Configuration;

/// <summary>
/// Настройки шаблонов email сообщений системы библиотеки
/// </summary>
public class EmailTemplateSettings
{
    public ReturnReminderTemplate ReturnReminder { get; set; } = new();
    public WeeklyReportTemplate WeeklyReport { get; set; } = new();
}

/// <summary>
/// Шаблон для email напоминаний о возврате книг
/// </summary>
public class ReturnReminderTemplate
{
    public string SubjectTemplate { get; set; } = "Напоминание о возврате книги: \"{BookTitle}\"";
    public int DaysBeforeDueDate { get; set; } = 3;
    public string LibraryName { get; set; } = "Библиотека";
    public string LibraryAddress { get; set; } = "";
    public string LibraryPhone { get; set; } = "";
    public string WorkingHours { get; set; } = "";
}

/// <summary>
/// Шаблон для email с еженедельными отчетами
/// </summary>
public class WeeklyReportTemplate
{
    public string SubjectTemplate { get; set; } = "Еженедельный отчет библиотеки за период {StartDate} - {EndDate}";
    public string[] AdminEmails { get; set; } = Array.Empty<string>();
    public int ReportRetentionDays { get; set; } = 90;
}
