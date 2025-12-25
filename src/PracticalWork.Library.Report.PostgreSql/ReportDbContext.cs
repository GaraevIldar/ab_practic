using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Report.PostgreSql.Entity;

namespace PracticalWork.Library.Report.PostgreSql;

/// <summary>
/// Контекст EF Core для работы с отчетами
/// </summary>
public class ReportDbContext(DbContextOptions<ReportDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ReportDbContext).Assembly);
    }

    #region Set UpdateDate on SaveChanges
    
    // Данные перегрузки выбраны потому, что оставшиеся две вызывают эти методы:

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetUpdateDates();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetUpdateDates();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetUpdateDates()
    {
        var updateDate = DateTime.UtcNow;

        var updatedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in updatedEntries)
        {
            if (entry.Entity is IEntity entity)
                entity.UpdatedAt = updateDate;
        }
    }

    #endregion

    public DbSet<ReportEntity> Reports { get; set; }
    public DbSet<ActivityLogEntity> ActivityLogs { get; set; }
}