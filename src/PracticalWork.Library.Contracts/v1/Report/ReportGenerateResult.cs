namespace PracticalWork.Library.Contracts.v1.Report;

/// <summary>
/// Объект результата сгенерированного отчета
/// </summary>
/// <param name="Content">Поток байтов, представляющий контент отчета</param>
/// <param name="ContentType">Тип контента отчетаП</param>
///  <param name="FileName">Название файла отчета</param>
public record ReportGenerateResult(
    Stream Content, 
    string ContentType, 
    string FileName);