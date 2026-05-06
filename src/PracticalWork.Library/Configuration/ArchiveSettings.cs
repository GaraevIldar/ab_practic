namespace PracticalWork.Library.Configuration;

/// <summary>
/// Настройки автоматической архивации старых книг
/// </summary>
public class ArchiveSettings
{
    public int YearsWithoutBorrow { get; set; } = 3;
    public int MaxBooksPerRun { get; set; } = 100;
}
