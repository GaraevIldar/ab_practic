using MessageBroker.RabbitMQ.Data.PostgreSql.Entities;
using Microsoft.EntityFrameworkCore;

namespace MessageBroker.RabbitMQ.Data.PostgreSql;

public class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> options)
        : base(options) {}

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Report> Reports => Set<Report>();
}
