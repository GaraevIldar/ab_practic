using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Pagination;
using PracticalWork.Library.Models;
using PracticalWork.Library.Report.PostgreSql.Entity;

namespace PracticalWork.Library.Report.PostgreSql.Repositories;

public class ActivityLogRepository: IActivityLogRepository
{
    private readonly ReportDbContext _context;

    public ActivityLogRepository(ReportDbContext context)
    {
        _context = context;
    }


    public async Task AddLogAsync(ActivityLog activityLog)
    {
        ActivityLogEntity entity = new()
        {
            EventDate = activityLog.EventDate,
            EventType = activityLog.EventType,
            Metadata = activityLog.SerializeEvent()
        };
        _context.ActivityLogs.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ActivityLog>> GetLogsPageAsync(ActivityLogsPaginationRequest request)
    {
        DateTime? fromUtc = request.EventDateFrom.HasValue
            ? request.EventDateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            : null;

        DateTime? toUtc = request.EventDateTo.HasValue
            ? request.EventDateTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc)
            : null;

        var logs = await _context.ActivityLogs
            .AsNoTracking()
            .Where(l => !fromUtc.HasValue || l.EventDate >= fromUtc.Value)
            .Where(l => !toUtc.HasValue || l.EventDate <= toUtc.Value)
            .Where(l => request.EventType == null
                        || request.EventType.Length == 0
                        || request.EventType.Contains(l.EventType))
            .ToListAsync();

        return logs.Select(entity => new ActivityLog
        {
            EventType = entity.EventType,
            EventDate = entity.EventDate,
            Event = ActivityLog.DeserializeEvent(entity.EventType, entity.Metadata)
        }).ToList();
    }

    public async Task<IReadOnlyList<ActivityLog>> GetLogsAsync(DateOnly? periodFrom, DateOnly? periodTo, string[] eventTypes)
    {
        DateTime? dtFrom = periodFrom?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc); 
        DateTime? dtTo = periodTo?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var entities = _context.ActivityLogs
            .Where(l => eventTypes == null || eventTypes.Length == 0 || eventTypes.Contains(l.EventType))
            .Where(l => !dtFrom.HasValue || l.EventDate >= dtFrom)
            .Where(l => !dtTo.HasValue || l.EventDate <= dtTo)
            .Select(l => new { l.EventDate, l.EventType, l.Id, l.Metadata });
        
        return await entities
            .Select(e => new ActivityLog
            {
                EventDate = e.EventDate, 
                EventType = e.EventType, 
                Event = ActivityLog.DeserializeEvent(e.EventType, e.Metadata)
            })
            .ToListAsync();
    }
}