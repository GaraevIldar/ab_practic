using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Report.PostgreSql.Entity;

public class ReportEntity: EntityBase
{
    public string Name { get; set; }
    public string FilePath { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public ReportStatus Status { get; set; }
    public IReadOnlyList<string> EventTypes { get; set; }
}