using Microsoft.EntityFrameworkCore;
using Npgsql;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Report.PostgreSql.Entity;

namespace PracticalWork.Library.Report.PostgreSql.Repositories;

public class ReportRepository: IReportRepository
{
    private readonly ReportDbContext _reportDbContext;

    public ReportRepository(ReportDbContext reportDbContext)
    {
        _reportDbContext = reportDbContext;
    }
    public async Task<Guid> CreateReport(Models.Report report)
    {
        var entity = new ReportEntity
        {
            PeriodFrom = report.PeriodFrom,
            PeriodTo = report.PeriodTo,
            EventTypes = report.EventTypes,
            Status = report.Status,
        };
        _reportDbContext.Reports.Add(entity);
        await _reportDbContext.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<IReadOnlyList<Models.Report>> GetReadyReports()
    {
        var entities = await _reportDbContext.Reports
            .Where(r => r.Status == ReportStatus.Generated)
            .ToListAsync();
        
        return entities
            .Select(e => new Models.Report
            {
                Name = e.Name,
                EventTypes = e.EventTypes,
                GeneratedAt = e.GeneratedAt,
                PeriodFrom = e.PeriodFrom,
                PeriodTo = e.PeriodTo,
            })
            .ToList();
    }

    public async Task<Models.Report> GetReportById(Guid reportId)
    {
        var reportEntity = await _reportDbContext.Reports
            .SingleOrDefaultAsync(r => r.Id == reportId);

        return new Models.Report
        {
            Name = reportEntity.Name,
            EventTypes = reportEntity.EventTypes,
            GeneratedAt = reportEntity.GeneratedAt,
            PeriodFrom = reportEntity.PeriodFrom,
            PeriodTo = reportEntity.PeriodTo,
            Status = reportEntity.Status,
            FilePath = reportEntity.FilePath,
        };
    }

    public async Task<(Guid id, Models.Report report)> GetReportByName(string reportName)
    {
        var reportEntity = await _reportDbContext.Reports
            .SingleOrDefaultAsync(r => r.Name == reportName); 

        return (reportEntity.Id, new Models.Report
        {
            Name = reportEntity.Name,
            EventTypes = reportEntity.EventTypes,
            GeneratedAt = reportEntity.GeneratedAt,
            PeriodFrom = reportEntity.PeriodFrom,
            PeriodTo = reportEntity.PeriodTo,
            Status = reportEntity.Status,
            FilePath = reportEntity.FilePath,
        });
    }

    public async Task UpdateReport(Guid reportId, Models.Report report)
    {
        var reportEntity = await _reportDbContext.Reports
            .SingleOrDefaultAsync(r => r.Id == reportId);
        
        reportEntity.Name = report.Name;
        reportEntity.EventTypes = report.EventTypes;
        reportEntity.GeneratedAt = report.GeneratedAt;
        reportEntity.PeriodFrom = report.PeriodFrom;
        reportEntity.PeriodTo = report.PeriodTo;
        reportEntity.Status = report.Status;
        reportEntity.FilePath = report.FilePath;
        
        await _reportDbContext.SaveChangesAsync();
    }
}