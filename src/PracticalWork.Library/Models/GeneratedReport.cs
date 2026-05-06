namespace PracticalWork.Library.Models;

/// <summary>
/// Результат генерации отчета (для еженедельного задания)
/// </summary>
public sealed class GeneratedReport
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string DownloadUrl { get; set; }
    public DateTime GeneratedAt { get; set; }
}
