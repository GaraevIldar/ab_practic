namespace PracticalWork.Library.Exceptions.Report;

public sealed class ReportNotFoundException: AppException
{ 
        public ReportNotFoundException(Guid id)
        : base($"Отчета с ID {id} не найден") { }
}
