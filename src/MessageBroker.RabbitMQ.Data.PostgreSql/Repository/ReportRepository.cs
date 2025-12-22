using MessageBroker.RabbitMQ.Data.PostgreSql.Entities;
using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Models;
using Report = MessageBroker.RabbitMQ.Data.PostgreSql.Entities.Report;

namespace MessageBroker.RabbitMQ.Data.PostgreSql.Repository;

public class ReportRepository : IReportRepository
{
    private readonly ReportsDbContext _reportDbContext;

    public ReportRepository(ReportsDbContext reportDbContext)
    {
        _reportDbContext = reportDbContext;
    }

    public async Task GetAllAsync()
    {
        await _reportDbContext.Reports.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ReportMetadata report)
    {
        _reportDbContext.Reports.Add(report);
        await _reportDbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string fileName) =>
        await _reportDbContext.Reports.AnyAsync(x => x.FileName == fileName);
}
