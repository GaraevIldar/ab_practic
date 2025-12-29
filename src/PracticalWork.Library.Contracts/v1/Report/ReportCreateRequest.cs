namespace PracticalWork.Library.Contracts.v1.Report;

/// <summary>
/// Создание отчета
/// </summary>
/// <param name="PeriodFrom"> фильтр по времени отсчета</param>
/// <param name="PeriodTo">фильтр по времени окончания</param>
/// <param name="EventTypes">фильтр на типы событий</param>
public record ReportCreateRequest(
    DateOnly PeriodFrom, 
    DateOnly PeriodTo, 
    IReadOnlyList<string> EventTypes);