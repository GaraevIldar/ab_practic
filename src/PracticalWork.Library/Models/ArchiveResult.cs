namespace PracticalWork.Library.Models;

/// <summary>
/// Результат запуска задачи архивации книг
/// </summary>
public sealed class ArchiveResult
{
    public int TotalProcessed { get; set; }
    public int Archived { get; set; }
    public int Skipped { get; set; }
    public List<string> SkipReasons { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
}
